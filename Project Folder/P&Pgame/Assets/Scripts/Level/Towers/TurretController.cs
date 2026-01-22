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

        Vector3 predictedPoint = targetPoint; // Start with the current target point
        int iterations = 2; // 2 or 3 iterations usually provides sufficient accuracy

        for (int i = 0; i < iterations; i++)
        {
            float distanceToPredicted = Vector3.Distance(predictedPoint, shootPos.position);
            float iterateTime = distanceToPredicted / (bulletSpeed > 0 ? bulletSpeed : 100f);

            // Recalculate the predicted point based on the new travel time
            predictedPoint = targetPoint + (enemyVelocity * iterateTime);
        }

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

        // Instantiate the bullet with the CombinedBulletScript attached
        GameObject newBulletGO = Instantiate(bulletPrefab, shootPos.position, turret.rotation); 

        Collider[] bulletColliders = newBulletGO.GetComponentsInChildren<Collider>();

        foreach (var bulletCol in bulletColliders)
        {
            Physics.IgnoreCollision(bulletCol, GetComponent<Collider>());
        }

        Rigidbody bulletRB = newBulletGO.GetComponent<Rigidbody>();

        if (bulletRB != null)
        {
            // 1. Ensure the bullet isn't fighting itself
            bulletRB.linearVelocity = Vector3.zero;
            bulletRB.angularVelocity = Vector3.zero;

            // Apply speed directly using VelocityChange to ignore mass
            bulletRB.AddForce(turret.forward * bulletSpeed, ForceMode.VelocityChange);

            Debug.Log($"Firing with Force: {turret.forward * bulletSpeed} | Turret Forward: {turret.forward}");
        }
    }

    public void takeDamage(float amount, DamageType type)
    {
        HP -= (int) amount;

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