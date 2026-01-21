using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "New Projectile Data", menuName = "Turret/Projectile Data")]
public class ProjectileData : ScriptableObject
{
    public float Speed = 20f;
    public int DamageAmount = 10;
    // Add other shared data here if needed (e.g., AoE radius)
}

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] public ProjectileData Data; // Link your SO asset here
    private Rigidbody rb;
    private bool hasCollided = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Start flying straight

        // Remove the complex mesh/trail code for simplicity, 
        // or move it into a helper function if you need it later.

        // Manage lifetime (basic deletion after 5 seconds)
        // This simulates the async deletion logic from the PDF script
        StartCoroutine(HandleDeletionAfterTime(5f));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return;
        hasCollided = true;
        rb.useGravity = true; // Drop after collision

        // Handle the damage logic here or via the damage script
        // collision.gameObject.GetComponent<IDamage>()?.takeDamage(Data.DamageAmount);

        // This is where your AoE turret might use a different method if needed
        if (Data.DamageAmount > 0)
        {
            // Call damage logic (if you want the damage script on the bullet itself, keep it there)
        }
    }

    IEnumerator HandleDeletionAfterTime(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}

public class TurretController : MonoBehaviour, IDamage
{
    [Header("Stats")]
    [SerializeField] Renderer model;
    [SerializeField] int numEnemies;
    [Range(1, 1000)][SerializeField] int HP = 1000;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform turret; 
    [SerializeField] GameObject bulletPrefab; // Reference the prefab with the Projectile script
    [SerializeField] ProjectileData projectileData; // Reference the ScriptableObject directly here

    [Range(0, 5)][SerializeField] float shootRate;
    [Range(1, 1000)][SerializeField] int shootDist;

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

        if (projectileData != null)
        {
            bulletSpeed = projectileData.Speed;
        }
        else
        {
            Debug.LogError("ProjectileData asset not assigned!");
            bulletSpeed = 20f; // Default if missing
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
    void Update()
    {
        shootTimer += Time.deltaTime;
        Debug.DrawRay(shootPos.position, shootPos.forward * shootDist, Color.red);
        enemiesInRange.RemoveAll(enemy => enemy == null);
        numEnemies = enemiesInRange.Count;

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
        Vector3 targetPoint = other.bounds.center;

        // 1. Try to get velocity from NavMeshAgent or Rigidbody
        Vector3 enemyVelocity = Vector3.zero;
        NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
        Rigidbody rb = other.GetComponent<Rigidbody>();

        if (agent != null) enemyVelocity = agent.velocity;
        else if (rb != null) enemyVelocity = rb.linearVelocity;

        // 2. Calculate Lead Aim
        float distance = Vector3.Distance(targetPoint, shootPos.position);

        // Avoid division by zero if bulletSpeed isn't set
        float travelTime = distance / (bulletSpeed > 0 ? bulletSpeed : 100f);

        // 3. The Predicted Position
        Vector3 predictedPoint = targetPoint + (enemyVelocity * travelTime);

        // 4. Calculate direction to the PREDICTED point
        Vector3 fullDirection = predictedPoint - shootPos.position;

        if (fullDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(fullDirection);

            turret.rotation = Quaternion.RotateTowards(
                turret.rotation,
                targetRotation,
                Time.deltaTime * 100
            );
        }
    }

    void Shoot()
    {
        Debug.DrawRay(shootPos.position, turret.forward * 5f, Color.yellow, 2f);
        shootTimer = 0;
        GameObject newBulletGO = Instantiate(bulletPrefab, shootPos.position, turret.rotation);
        Rigidbody bulletRB = newBulletGO.GetComponent<Rigidbody>();

        if (bulletRB != null)
        {
            // 1. Ensure the bullet isn't fighting itself
            bulletRB.linearVelocity = Vector3.zero;
            bulletRB.angularVelocity = Vector3.zero;

            Vector3 force = turret.forward * projectileData.Speed;
            Debug.Log($"Firing with Force: {force} | Turret Forward: {turret.forward}");
            bulletRB.AddForce(force, ForceMode.VelocityChange);
            // 2. Apply speed directly using VelocityChange to ignore mass
            //bulletRB.AddForce(turret.forward * projectileData.Speed, ForceMode.VelocityChange);
        }
    }

    public void takeDamage(int amount)
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


