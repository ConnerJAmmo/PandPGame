using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace bullet.fx.pack
{
    // Enum to differentiate between the two potential mesh types (from visual script)
    public enum MeshType { Bullet, Cylinder }
    // Enum from the original damage script
    public enum DamageType { moving, stationary, DOT }

    // Main damage logic script
    public sealed class damage : MonoBehaviour
    {
        [Header("Debug")]
        public Transform target;

        [Header("Components")]
        [SerializeField] public GameObject dotZonePrefab;
        [SerializeField] public GameObject hitEffectPrefab;
        [SerializeField] public LayerMask damageLayers;
        [SerializeField] public LayerMask blockLayers;

        [Header("Stats")]
        [SerializeField] public DamageType type = DamageType.moving;
        [SerializeField] public float lifeTime;
        [SerializeField] public float speed;
        [SerializeField] public int damageAmount;
        [SerializeField] public float dotDamageRate;
        [SerializeField] public bool leavesDotZone = false;
        [SerializeField] public bool enableHoming = false;
        [SerializeField] public bool createHitEffect = false;
        [SerializeField] public bool groundedHitEffect = false;
        [SerializeField] public bool destroyOnHit = true;

        [Header("Bullet Visuals")]
        [SerializeField] public bool bulletProjectile = false;
        [SerializeField] public BulletEffectType bulletEffectType;
        [SerializeField] public MeshType meshType;
        [SerializeField] public Transform endPosition;
        [SerializeField] public GameObject fireCylinder;
        [SerializeField] public GameObject fireEffect;

        private float rotationSpeed = 450f;
        private float groundCheckDistance = 10f;
        private LayerMask groundLayer;
        private BulletVisualFX visualFX;
        private readonly HashSet<IDamage> dotVictims = new HashSet<IDamage>();

        // Reset() method for editor auto-setup
        void Reset()
        {
            var col = GetComponent<Collider>();
            if (col) col.isTrigger = true;
            if (GetComponent<Rigidbody>() != null)
            {
                GetComponent<Rigidbody>().useGravity = false;
                GetComponent<Rigidbody>().isKinematic = (type != DamageType.moving);
            }
        }
        // Awake() method to ensure trigger is set at runtime
        void Awake()
        {
            var col = GetComponent<Collider>();
            if (col) col.isTrigger = true;
        }

        void Start()
        {
            visualFX = GetComponent<BulletVisualFX>();

            // Original Start logic from damage.cs for movement
            if (type == DamageType.moving)
            {
                if (GetComponent<Rigidbody>() != null)
                {
                    GetComponent<Rigidbody>().isKinematic = false;
                    GetComponent<Rigidbody>().linearVelocity = transform.forward * speed;
                }
            }
            
            if (lifeTime > 0f)
            {
                Destroy(gameObject, lifeTime); // Destroy after lifetime
            }

            // Start visuals logic via delegation
            if (bulletProjectile && visualFX != null)
            {
                visualFX.StartVisualEffects(bulletEffectType, endPosition,
                    fireCylinder, fireEffect, meshType, createHitEffect, 
                    hitEffectPrefab, groundedHitEffect, groundCheckDistance, groundLayer);
            }
        }

        // FixedUpdate logic remains here as it deals with physics (homing)
        void FixedUpdate()
        {
            if (visualFX != null) visualFX.HandleDeletionTimer();
            if (!enableHoming || target == null || GetComponent<Rigidbody>() == null) return;
            // Calculate the direction to the target
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            // Calculate the rotation needed to face the target smoothly
            Vector3 currentDirection = transform.forward;
            Vector3 resultingDirection = Vector3.RotateTowards(currentDirection, directionToTarget,
                rotationSpeed * Mathf.Deg2Rad * Time.deltaTime, 1f);
            transform.rotation = Quaternion.LookRotation(resultingDirection);
            // Maintain forward movement at the specified speed
            GetComponent<Rigidbody>().linearVelocity = transform.forward * speed;
        }

        // --- OnTriggerEnter (from original damage.cs) ---
        private async void OnTriggerEnter(Collider other)
        {
            // Set IsFlying to false upon any trigger/collision
            if (visualFX != null) visualFX.SetFlying(false);

            int otherLayerMask = 1 << other.gameObject.layer;
            Vector3 hitPoint = GetSafeContactPoint(other);

            // Check block layers first (walls, ground) - if it hits this, we do hit FX and despawn
            if ((blockLayers.value & otherLayerMask) != 0)
            {
                if (visualFX != null) visualFX.DoHitFX(hitPoint, createHitEffect, hitEffectPrefab,
                groundedHitEffect, groundCheckDistance, groundLayer);
                if (type == DamageType.moving && destroyOnHit)
                {
                    // Removed the 5-second delay, destroy immediately
                    Destroy(gameObject);
                }
                return; // Exit the function if we hit a wall/blocker
            }

            // If is not a damageable layer ignore it
            if ((damageLayers.value & otherLayerMask) == 0)
                return;

            // If it is a damageable layer, call the unified logic
            HandleImpactLogic(other.gameObject, hitPoint);
        }

        Vector3 GetSafeContactPoint(Collider other)
        {
            // If it is a MeshCollider and not convex, use ClosestPointOnBounds
            if (other is MeshCollider meshCol && !meshCol.convex)
            {
                return other.ClosestPointOnBounds(transform.position);
            }
            // For all other supported colliders (Box, Sphere, Capsule, convex Mesh), use ClosestPoint.
            // ClosestPointOnBounds works as a reliable fallback for any collider type.
            return other.ClosestPointOnBounds(transform.position);
        }

        // --- Unified Impact Logic Method ---
        private async void HandleImpactLogic(GameObject other, Vector3 hitPoint)
        {
            // Try to find the damage interface on the object or its parents
            IDamage dmg = other.GetComponent<IDamage>();
            if (dmg == null)
            {
                dmg = other.GetComponentInParent<IDamage>();
            }
            if (dmg == null)
            {
                // If we hit something non-damageable but didn't return via block layers, destroy
                // immediately if moving
                if (type == DamageType.moving && destroyOnHit)
                {
                    Destroy(gameObject);
                }
                return; // Exit the function if no target is found
            }

            // Apply damage logic based on Type
            if (type == DamageType.moving || type == DamageType.stationary)
            {
                // Check if the Rigidbody exists AND if the type is 'moving' before trying to set velocity
                if (GetComponent<Rigidbody>() != null && type == DamageType.moving)
                {
                    GetComponent<Rigidbody>() .linearVelocity = Vector3.zero;
                }

                dmg.takeDamage(damageAmount, DamageType.moving);
                HandleProjectileImpact(hitPoint); // Spawn DOT on the enemy if needed
                if (visualFX != null) visualFX.DoHitFX(hitPoint, createHitEffect, hitEffectPrefab,
                groundedHitEffect, groundCheckDistance, groundLayer);
                if (type == DamageType.moving && destroyOnHit)
                {
                    // Removed the 5-second delay, destroy immediately
                    Destroy(gameObject);
                }
            }
            else if (type == DamageType.DOT)
            {
                if (!dotVictims.Contains(dmg))
                {
                    dotVictims.Add(dmg);
                    StartCoroutine(DotDamage(dmg));
                }
            }
        }

        // --- Remaining Damage-related helper methods ---
        // OnTriggerExit logic (damage-related)
        private void OnTriggerExit(Collider other)
        {
            if (type != DamageType.DOT) return;
            IDamage dmg = other.GetComponent<IDamage>();
            if (dmg == null) dmg = other.GetComponentInParent<IDamage>();
            if (dmg != null) dotVictims.Remove(dmg);
        }

        // Handles spawning a persistent DOT zone on hit (damage-related logic)
        void HandleProjectileImpact(Vector3 point)
        {
            if (type == DamageType.moving && leavesDotZone && dotZonePrefab != null)
            {
                Vector3 spawnPos = point;
                if (groundedHitEffect)
                {
                    Vector3 rayStart = point + Vector3.up * 0.1f;
                    if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
                    {
                        spawnPos = hit.point;
                    }
                }
                Instantiate(dotZonePrefab, spawnPos, Quaternion.identity);
            }
        }

        // The Coroutine for applying damage over time
        IEnumerator DotDamage(IDamage target)
        {
            while (target != null && dotVictims.Contains(target))
            {
                if (target != null && !(target as UnityEngine.Object).Equals(null))
                {
                    target.takeDamage(damageAmount, DamageType.DOT);
                }
                else
                {
                    dotVictims.Remove(target);
                    yield break;
                }
                yield return new WaitForSeconds(dotDamageRate);
            }
            if (target != null) dotVictims.Remove(target);
        }

        // Helper to get contact points correctly
        Vector3 GetSafeContactPoint(Collider other, Vector3 defaultPoint)
        {
            if (other is MeshCollider meshCol && !meshCol.convex)
            {
                return other.ClosestPointOnBounds(transform.position);
            }
            return other.ClosestPoint(transform.position);
        }
    }
}

