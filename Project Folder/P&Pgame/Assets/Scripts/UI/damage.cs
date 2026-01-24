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
        // --- Core Components & Damage Type (from original damage.cs) ---
        [Header("Type (Legacy)")]
        [SerializeField] DamageType Type = DamageType.moving;
        [Header("Core Components")]
        [SerializeField] private Rigidbody Rigidbody; // RB from visual script is used here
        // --- Projectile Movement (from original damage.cs) ---
        [Header("Projectile Movement")]
        [SerializeField] float lifeTime = 5f;
        [SerializeField] public float speed;
        // --- DOT Settings (Only if we are using DOT) (from original damage.cs) ---
        [Header("DOT Settings (Only if we are using DOT)")]
        [SerializeField] float damageRate;
        // --- Projectile DOT Settings (from original damage.cs) ---
        [Header("Projectile DOT Settings")]
        [SerializeField] bool leavesDotZone = false;
        [SerializeField] GameObject dotZonePrefab; // A prefab with this script set to "DOT"
        // --- Damage (from original damage.cs) ---
        [Header("Damage")]
        [SerializeField] int damageAmount;
        // --- Hit Rules (from original damage.cs) ---
        [Header("Hit Rules")]
        [SerializeField] bool destroyOnHit = true;
        [SerializeField] bool createHitEffect = true;
        [SerializeField] bool groundedHitEffect = false;
        public float groundCheckDistance = 10f; // Max distance to look for ground
        public LayerMask groundLayer; // Layer filter for the ground
        [SerializeField] GameObject hitEffectPrefab;
        // --- Layer Rules (from original damage.cs) ---
        [Header("Layer Rules")]
        [SerializeField] LayerMask damageLayers;
        [SerializeField] LayerMask blockLayers; // Walls, Terrain, Ground
        // --- Homing Settings (Opt-In) ---
        [Header("Homing Settings")]
        [SerializeField] private bool enableHoming = false; // Check this box for homing projectiles
        [SerializeField] private float rotationSpeed = 100f; // How fast the bullet turns towards the target
        public Transform target; // The target to follow
        // --- Visual Effects (Opt-In Settings) - these fields are only to signal the visual script if enabled ---
        [Header("Visual Effects (Opt-In Settings)")]
        [SerializeField] private bool enableVisualEffects = false; // Main opt-in flag
        [SerializeField] private BulletEffectType BulletEffectType;
        [SerializeField] private Transform EndPosiotionBullet;
        [SerializeField] private Material BulletTrailMaterial;
        [SerializeField] private GameObject Fire2Effect;
        [SerializeField] private GameObject Fire3Effect;
        [SerializeField] private MeshType meshType;

        // --- Private variables for damage/logic (from original damage.cs) ---
        private readonly HashSet<IDamage> dotVictims = new HashSet<IDamage>();
        private bool hasCollided = false; // Added to prevent multiple hits

        // Reference to the new visual component
        private BulletVisualFX visualFX;

        // Reset() method for editor auto-setup
        void Reset()
        {
            var col = GetComponent<Collider>();
            if (col) col.isTrigger = true;
            Rigidbody = GetComponent<Rigidbody>();
            if (Rigidbody != null)
            {
                Rigidbody.useGravity = false;
                Rigidbody.isKinematic = (Type != DamageType.moving);
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
            if (Type == DamageType.moving)
            {
                if (Rigidbody != null)
                {
                    Rigidbody.linearVelocity = transform.forward * speed;
                }
                if (lifeTime > 0f)
                {
                    Destroy(gameObject, lifeTime); // Destroy after lifetime
                }
            }

            // Start visuals logic via delegation
            if (enableVisualEffects && visualFX != null)
            {
                visualFX.StartVisualEffects(BulletEffectType, EndPosiotionBullet, BulletTrailMaterial, Fire2Effect, Fire3Effect, meshType, createHitEffect, hitEffectPrefab, groundedHitEffect, groundCheckDistance, groundLayer);
            }
        }

        // FixedUpdate logic remains here as it deals with physics (homing)
        void FixedUpdate()
        {
            if (visualFX != null) visualFX.HandleDeletionTimer();
            if (!enableHoming || target == null || Rigidbody == null) return;
            // Calculate the direction to the target
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            // Calculate the rotation needed to face the target smoothly
            Vector3 currentDirection = transform.forward;
            Vector3 resultingDirection = Vector3.RotateTowards(currentDirection, directionToTarget,
                rotationSpeed * Mathf.Deg2Rad * Time.deltaTime, 1f);
            transform.rotation = Quaternion.LookRotation(resultingDirection);
            // Maintain forward movement at the specified speed
            Rigidbody.linearVelocity = transform.forward * speed;
        }

        // --- OnTriggerEnter (from original damage.cs) ---
        private async void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger) return; // not activated by another trigger
            int otherLayerMask = 1 << other.gameObject.layer;

            // Set Rigidbody gravity to true and IsFlying to false upon any collision/trigger
            if (Rigidbody != null) Rigidbody.useGravity = true;
            if (visualFX != null) visualFX.SetFlying(false);

            // If it hits a block layer, handle impact, FX, and set for deletion
            if ((blockLayers.value & otherLayerMask) != 0)
            {
                Vector3 hitPoint = other.ClosestPointOnBounds(transform.position); ;
                if (visualFX != null) visualFX.DoHitFX(hitPoint, createHitEffect, hitEffectPrefab, groundedHitEffect, groundCheckDistance, groundLayer);
                if (Type == DamageType.moving && destroyOnHit)
                {
                    await Task.Delay(5000); // Wait for visual fade
                    if (visualFX != null) visualFX.InitiateDeletion();
                }
                return;
            }

            // If is not a damageable layer ignore it
            if ((damageLayers.value & otherLayerMask) == 0)
                return;

            // Determine hit point for damageable object
            Vector3 dmgHitPoint;
            if (other is MeshCollider meshCol && !meshCol.convex)
            {
                dmgHitPoint = other.ClosestPointOnBounds(transform.position);
            }
            else
            {
                dmgHitPoint = other.ClosestPoint(transform.position);
            }

            // Call unified logic
            HandleImpactLogic(other.gameObject, dmgHitPoint);
        }

        // --- OnCollisionEnter (from original damage.cs) ---
        private void OnCollisionEnter(Collision collision)
        {
            // The hasCollided flag prevents multiple hits from the same fast-moving bullet
            if (hasCollided) return;
            hasCollided = true;
            if (visualFX != null) visualFX.SetFlying(false);

            int otherLayerMask = 1 << collision.gameObject.layer;

            // Determine a safe hit point using the corrected logic for all collider types
            Vector3 hitPoint = GetSafeContactPoint(collision.collider, collision.contacts[0].point);

            // Check block layers first (walls, ground) - if it hits this, we do hit FX and despawn
            if ((blockLayers.value & otherLayerMask) != 0)
            {
                if (visualFX != null) visualFX.DoHitFX(hitPoint, createHitEffect, hitEffectPrefab, groundedHitEffect, groundCheckDistance, groundLayer);
                if (Type == DamageType.moving && destroyOnHit)
                {
                    Destroy(gameObject);
                }
                return;
            }

            // Call unified logic
            HandleImpactLogic(collision.gameObject, hitPoint);
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
                // If we hit something non-damageable but didn't return via block layers, destroy immediately if moving
                if (Type == DamageType.moving && destroyOnHit)
                {
                    Destroy(gameObject);
                }
                return; // Exit the function if no target is found
            }

            // Apply damage logic based on Type
            if (Type == DamageType.moving || Type == DamageType.stationary)
            {
                dmg.takeDamage(damageAmount, DamageType.moving);

                HandleProjectileImpact(hitPoint); // Spawn DOT on the enemy if needed

                if (visualFX != null) visualFX.DoHitFX(hitPoint, createHitEffect, hitEffectPrefab, groundedHitEffect, groundCheckDistance, groundLayer);

                if (Type == DamageType.moving && destroyOnHit)
                {
                    // Use async Task.Delay to wait for the visual effect fade before actual deletion
                    await Task.Delay(5000);
                    if (visualFX != null) visualFX.InitiateDeletion();
                }
            }
            else if (Type == DamageType.DOT)
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
            if (Type != DamageType.DOT) return;
            IDamage dmg = other.GetComponent<IDamage>();
            if (dmg == null) dmg = other.GetComponentInParent<IDamage>();
            if (dmg != null) dotVictims.Remove(dmg);
        }

        // Handles spawning a persistent DOT zone on hit (damage-related logic)
        void HandleProjectileImpact(Vector3 point)
        {
            if (Type == DamageType.moving && leavesDotZone && dotZonePrefab != null)
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
                yield return new WaitForSeconds(damageRate);
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

