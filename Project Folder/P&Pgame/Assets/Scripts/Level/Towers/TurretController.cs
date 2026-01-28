using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Threading.Tasks;
using bullet.fx.pack;

public class TurretController : MonoBehaviour, IDamage
{
    [Header("Debug")]
    [SerializeField] public int numEnemies;
    [SerializeField] public Collider target;

    [Header("Components")]
    [SerializeField] public GameObject turret;
    [SerializeField] public GameObject bulletPrefab;

    [Header("Stats")]
    [Range(1, 1000)] [SerializeField] public int HP;

    [Header("Fire Settings")]
    [Range(1, 1000)] [SerializeField] public int range;
    [Range(1, 5)] [SerializeField] public int shotsPerBurst;
    [Range(0, 2)] [SerializeField] public float burstFireRate;
    [Range(0.01f, 2)] [SerializeField] public float fireRate;

    [Header("Idle Scan Settings")]
    [SerializeField] public float scanSpeed;
    [SerializeField] public float scanAngle;

    public TurretFireManager fireManager;

    private bool useBurstFire;
    private float shootTimer;
    private float bulletSpeed;
    private Color colorOrigin;
    private Quaternion forward;
    private Vector3 fullDirection;
    private Material dynamicMat;
    private Transform firePoint;
    private List<Collider> enemiesInRange = new List<Collider>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        forward = Quaternion.LookRotation(turret.transform.forward);
        dynamicMat = GetComponentInChildren<Renderer>().material;
        colorOrigin = dynamicMat.color;
        damage dmgScript = bulletPrefab.GetComponentInChildren<damage>();
        if (dmgScript != null)
        {
            bulletSpeed = dmgScript.speed;
        }
        else
        {
            Debug.LogError("Damage script missing on prefab at Start!", this);
        }
        if(shotsPerBurst == 1)
        {
            useBurstFire = false;
        }
        else 
        {             
            useBurstFire = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInRange.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInRange.Remove(other);
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        shootTimer += Time.deltaTime;
        enemiesInRange.RemoveAll(enemy => enemy == null);
        firePoint = fireManager.GetNextFirePoint();
        numEnemies = enemiesInRange.Count; //Debug Statement

        if (firePoint != null)
        {
            Debug.DrawRay(firePoint.position, firePoint.forward * range, Color.green, 0.1f);
            if (enemiesInRange.Count > 0)
            {
                // Get the precise aiming direction from the refactored function
                Vector3 preciseAimDirection = CalculatePreciseAimDirection(enemiesInRange[0].GetComponent<Collider>());

                // *** Turret Body Rotation (Smooth Visuals) ***
                // Slerp the main turret body smoothly towards the target center point for visual cohesion
                Vector3 targetDirectionSimple = (enemiesInRange[0].bounds.center - turret.transform.position).normalized;
                Quaternion targetRotationBody = Quaternion.LookRotation(targetDirectionSimple);
                turret.transform.rotation = Quaternion.Slerp(turret.transform.rotation, targetRotationBody, Time.deltaTime * 1.5f);
                // Use a low speed (e.g., 1.5f) for smooth visual tracking

                // *** Fire Point Rotation (Precise Aiming) ***
                // Snap or quickly Slerp the CURRENT fire point child object to the PRECISE interception direction
                Quaternion targetRotationFirePoint = Quaternion.LookRotation(preciseAimDirection);
                firePoint.rotation = Quaternion.RotateTowards(firePoint.rotation, targetRotationFirePoint, Time.deltaTime * 1000f);
                // Use a high speed (e.g., 1000f) to snap to aim instantly

                // *** Aiming Check (Use firePoint.forward) ***
                // Check alignment using the precise firePoint's forward direction
                if (Vector3.Dot(firePoint.forward.normalized, preciseAimDirection.normalized) > 0.99f) // Check the CHILD'S alignment
                {
                    if (shootTimer >= fireRate) //
                    {
                        Shoot(); // Only shoot when the specific fire point is aimed AND timer is ready
                    }
                }
            }
            else
            {
                // Procedural Idle Scan
                // Mathf.Sin creates a smooth wave from -1 to 1
                float angle = Mathf.Sin(Time.time * scanSpeed) * scanAngle;

                // Combine your 'forward' base rotation with the calculated scan angle
                Quaternion scanRotation = forward * Quaternion.Euler(0, angle, 0);

                // Smoothly rotate toward the scan position
                turret.transform.rotation = Quaternion.Slerp(turret.transform.rotation, scanRotation, Time.deltaTime * 2);
            }
        }
    }

    Vector3 CalculatePreciseAimDirection(Collider other)
    {
        float currentBulletSpeed = (bulletSpeed > 0) ? bulletSpeed : 50f;
        Vector3 targetPoint = other.bounds.center;
        Vector3 enemyVelocity = Vector3.zero;

        NavMeshAgent agent = other.GetComponentInParent<NavMeshAgent>(); // Check parent/children hierarchy if needed
        Rigidbody rb = other.GetComponentInParent<Rigidbody>(); // Check parent/children hierarchy if needed

        if (agent != null)
        {
            enemyVelocity = agent.velocity;
            //enemyVelocity.y = 0;
            //Debug.Log($"Enemy Velocity Magnitude: {enemyVelocity.magnitude} | Enemy Speed Parameter: {agent.speed}");
        }
        else if (rb != null)
        {
            enemyVelocity = rb.linearVelocity;
        }

        // Calculate initial prediction
        Vector3 predictedDirection = CalculateInterceptionDirection(firePoint.position, currentBulletSpeed, targetPoint, enemyVelocity);

        float leadDamping = 0.5f; // Start with 0.9f, adjust between 0.0f (no lead) and 1.0f (full lead)
        Vector3 currentDirection = (targetPoint - firePoint.position).normalized;
        // Blend the predicted direction with the current direction
        fullDirection = Vector3.Slerp(currentDirection, predictedDirection, leadDamping);
        Debug.DrawRay(firePoint.position, predictedDirection * range, Color.green, 0.1f); // Green is predicted
        Debug.DrawRay(firePoint.position, fullDirection * range, Color.red, 0.1f); // Red is actual aim direction
        return fullDirection;
        /*
        if (fullDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(fullDirection);

            turret.transform.rotation = Quaternion.RotateTowards(
                turret.transform.rotation,
                targetRotation,
                Time.deltaTime * 450f
            );
        }
        */
    }

    Vector3 CalculateInterceptionDirection(Vector3 shootPos, float bulletSpeed, Vector3 targetPos, Vector3 targetVelocity)
    {
        Vector3 targetRelativePosition = targetPos - shootPos;
        float t = 0f;

        // Law of Cosines interception formula (variables a, b, c remain the same)
        float a = Vector3.Dot(targetVelocity, targetVelocity) - (bulletSpeed * bulletSpeed);
        float b = 2f * Vector3.Dot(targetVelocity, targetRelativePosition);
        float c = Vector3.Dot(targetRelativePosition, targetRelativePosition);
        float determinant = b * b - 4f * a * c;

        if (determinant > 0f)
        {
            float t1 = (-b + Mathf.Sqrt(determinant)) / (2f * a);
            float t2 = (-b - Mathf.Sqrt(determinant)) / (2f * a);

            // *** THIS IS THE CRITICAL CHANGE ***
            // Select the smallest positive time 't'
            if (t1 > 0f && t2 > 0f)
            {
                t = Mathf.Min(t1, t2);
            }
            else
            {
                // If one is negative, use the positive one (or 0 if both are negative)
                t = Mathf.Max(t1, t2, 0f);
            }
        }
        else
        {
            // Fallback to direct travel time if no real solution (target too fast/far)
            t = targetRelativePosition.magnitude / bulletSpeed;
        }

        // Add a sanity check to ensure t is reasonable
        t = Mathf.Max(t, 0.01f); // Ensure time is slightly positive

        //Debug.Log($"Bullet Speed: {bulletSpeed}, Target Velocity: {targetVelocity.magnitude}, Time t: {t}, Predicted Pos: {targetRelativePosition + targetVelocity * t}");
        return (targetRelativePosition + targetVelocity * t).normalized;
    }

    void Shoot()
    {
        if (useBurstFire)
        {
            // If burst fire is enabled for this turret, start the coroutine
            StartCoroutine(FireBurstRoutine());
        }
        else
        {
            // If not using burst fire, fire a single shot
            shootTimer = 0; // Reset the timer immediately for the next single shot
            FireProjectile();
        }
    }

    // Helper method to handle the actual instantiation and physics setup of one bullet
    void FireProjectile()
    {
        Debug.DrawRay(firePoint.position, turret.transform.forward * 5f, Color.yellow, 2f);
        GameObject newBulletGO = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Collider[] bulletColliders = newBulletGO.GetComponentsInChildren<Collider>();

        foreach (var bulletCol in bulletColliders)
        {
            Physics.IgnoreCollision(bulletCol, GetComponent<Collider>());
        }

        Rigidbody bulletRB = newBulletGO.GetComponent<Rigidbody>();
        if (bulletRB == null)
        {
            Debug.LogError("Failed to get Rigidbody component on " + newBulletGO.name);
        }

        if (bulletRB != null)
        {
            // 1. Ensure the bullet isn't fighting itself
            bulletRB.linearVelocity = Vector3.zero;
            bulletRB.angularVelocity = Vector3.zero;

            Vector3 forceToApply = firePoint.forward * bulletSpeed;
            //Debug.Log($"Applying Force: {forceToApply.magnitude} m/s | Predicted Speed: {bulletSpeed}");
            bulletRB.AddForce(firePoint.forward * bulletSpeed, ForceMode.VelocityChange);
            //Debug.Log($"Firing with Force: {firePoint.forward * bulletSpeed} | Aim: {firePoint.forward}");
        }
    }

    IEnumerator FireBurstRoutine()
    {
        shootTimer = 0; // Reset timer here to define delay between bursts

        for (int i = 0; i < shotsPerBurst; i++)
        {
            FireProjectile(); // Call the helper method to fire the shot

            if (i < shotsPerBurst - 1)
            {
                yield return new WaitForSeconds(burstFireRate);
            }
        }
    }

    public void takeDamage(int amount, DamageType type)
    {
        HP -= amount;

        // Debug to prove it's this specific instance
        Debug.Log($"{gameObject.name} took {amount} damage. HP left: {HP}");

        if (HP <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            // Stop only the flash coroutine to prevent color getting stuck
            StopCoroutine(flashRed());
            StartCoroutine(flashRed());
        }
    }

    IEnumerator flashRed()
    {
        dynamicMat.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        dynamicMat.color = colorOrigin;
    }
}