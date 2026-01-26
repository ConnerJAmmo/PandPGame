using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Threading.Tasks;
using bullet.fx.pack;

public class TurretController : MonoBehaviour, IDamage
{
    [Header("Stats")]
    [SerializeField] Renderer model;
    [SerializeField] int numEnemies;
    [Range(1, 1000)][SerializeField] int HP = 1000;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform turret; 
    [SerializeField] GameObject bulletPrefab;

    [Range(0, 5)][SerializeField] float shootRate;
    [Range(1, 1000)][SerializeField] int shootDist; 
    [SerializeField] private bool usePredictiveAiming = false;
    [SerializeField] private bool useBurstFire = false; // Check this for burst fire turrets
    [Range(0.01f, 0.5f)][SerializeField] float timeBetweenShots = 0.1f; // Delay between shots in a burst
    [SerializeField] int shotsInBurst = 3; // Number of shots per burst

    [Header("Idle Scan Settings")]
    [SerializeField] float scanSpeed = 0.5f;
    [SerializeField] float scanAngle = 45f;

    Color colorOrigin;
    Material dynamicMat; // Store the unique instance material
    float shootTimer;
    [SerializeField] Collider target;
    Quaternion forward;

    private float bulletSpeed;
    private List<Collider> enemiesInRange = new List<Collider>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        forward = Quaternion.LookRotation(turret.transform.forward);
        dynamicMat = model.material;
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
        numEnemies = enemiesInRange.Count;
        Debug.DrawRay(shootPos.position, shootPos.forward * shootDist, Color.green, 0.1f);

        if (enemiesInRange.Count > 0)
        {
            faceTarget(enemiesInRange[0].transform.GetComponent<Collider>());

            if (shootTimer >= shootRate)
            {
                Shoot();
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
            turret.rotation = Quaternion.Slerp(turret.rotation, scanRotation, Time.deltaTime * 2);
        }
    }

    void faceTarget(Collider other)
    {
        float currentBulletSpeed = (bulletSpeed > 0) ? bulletSpeed : 50f;
        Vector3 targetPoint = other.bounds.center;
        Vector3 fullDirection;

        if (usePredictiveAiming)
        {
            Vector3 enemyVelocity = Vector3.zero;
            NavMeshAgent agent = other.GetComponentInParent<NavMeshAgent>(); // Check parent/children hierarchy if needed
            Rigidbody rb = other.GetComponentInParent<Rigidbody>(); // Check parent/children hierarchy if needed

            if (agent != null)
            {
                enemyVelocity = agent.velocity * 0.8f;
                enemyVelocity.y = 0;
                bool isStillMoving = !agent.pathPending &&
                         agent.remainingDistance > agent.stoppingDistance;

                if (enemyVelocity.sqrMagnitude < 0.1f && isStillMoving)
                {
                    enemyVelocity = other.transform.forward * agent.speed;
                }
                else if (!isStillMoving)
                {
                    // If they've reached the stop, force velocity to zero 
                    // to prevent "phantom" leading
                    enemyVelocity = Vector3.zero;
                }
                //Debug.Log($"Enemy Velocity Magnitude: {enemyVelocity.magnitude} | Enemy Speed Parameter: {agent.speed}");
            }
            else if (rb != null)
            {
                // Note: Rigidbody linearVelocity might be better if you have a recent Unity version, otherwise use .velocity
                enemyVelocity = rb.linearVelocity;
            }
            // Add a Debug statement here to see the calculated direction
            Vector3 predictedDirection = CalculateInterceptionDirection(shootPos.position, currentBulletSpeed, targetPoint, enemyVelocity);
            Debug.DrawRay(shootPos.position, predictedDirection * shootDist, Color.green, 0.1f);
            fullDirection = predictedDirection;
        }
        else
        {
            // Simple fallback: aim directly at the center of the target's bounds
            fullDirection = targetPoint - shootPos.position;
        }

        if (fullDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(fullDirection);

            turret.rotation = Quaternion.RotateTowards(
                turret.rotation,
                targetRotation,
                Time.deltaTime * 450f
            );
        }
    }

    // Helper function using analytical math to find the precise interception direction
    Vector3 CalculateInterceptionDirection(Vector3 shootPos, float bulletSpeed, Vector3 targetPos, Vector3 targetVelocity)
    {
        Vector3 targetRelativePosition = targetPos - shootPos;
        float t = 0f;

        // Law of Cosines interception formula
        float a = Vector3.Dot(targetVelocity, targetVelocity) - (bulletSpeed * bulletSpeed);
        float b = 2f * Vector3.Dot(targetVelocity, targetRelativePosition);
        float c = Vector3.Dot(targetRelativePosition, targetRelativePosition);

        float determinant = b * b - 4f * a * c;

        if (determinant > 0f)
        {
            float t1 = (-b + Mathf.Sqrt(determinant)) / (2f * a);
            float t2 = (-b - Mathf.Sqrt(determinant)) / (2f * a);
            t = Mathf.Max(t1, t2); // Use the positive time result
        }
        else
        {
            t = targetRelativePosition.magnitude / bulletSpeed; // Fallback to direct travel time
        }

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
        Debug.DrawRay(shootPos.position, turret.forward * 5f, Color.yellow, 2f);
        // shootTimer is handled by the main Shoot() wrapper or burst coroutine

        // Instantiate the bullet with the CombinedBulletScript attached
        GameObject newBulletGO = Instantiate(bulletPrefab, shootPos.position, turret.rotation);

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

            Vector3 forceToApply = turret.forward * bulletSpeed; // Capture the final vector

            // Log the speed used and the speed we thought we were using for prediction
            //Debug.Log($"Applying Force: {forceToApply.magnitude} m/s | Predicted Speed: {bulletSpeed}");
            // Apply speed directly using VelocityChange to ignore mass
            bulletRB.AddForce(turret.forward * bulletSpeed, ForceMode.VelocityChange);

            // Debug.Log($"Firing with Force: {turret.forward * bulletSpeed} | Turret Forward: {turret.forward}");
        }
    }

    IEnumerator FireBurstRoutine()
    {
        shootTimer = 0; // Reset timer here to define delay between bursts

        for (int i = 0; i < shotsInBurst; i++)
        {
            FireProjectile(); // Call the helper method to fire the shot

            if (i < shotsInBurst - 1)
            {
                yield return new WaitForSeconds(timeBetweenShots);
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