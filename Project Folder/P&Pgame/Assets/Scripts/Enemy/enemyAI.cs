using bullet.fx.pack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class enemyAI : MonoBehaviour, IDamage
{
    [Header("Components")]
    [SerializeField] Renderer model;
    [SerializeField] public NavMeshAgent agent;
    [SerializeField] public Rigidbody body;
    [SerializeField] GameObject target;
    [SerializeField] public Transform shootPos;
    [SerializeField] Transform headPos;
    [SerializeField] GameObject bullet;
    [SerializeField] LayerMask ignoreLayer;

    [Header("Stats")]
    [Range(1, 25)] [SerializeField] int HP;
    [Range(0, 2)] [SerializeField] float shootRate;
    [Range(1, 1000)] [SerializeField] int faceTargetSpeed;
    [Range(1, 1000)][SerializeField] int shootDist;
    [Range(0, 360)][SerializeField] int FOV = 90;
    [SerializeField] private bool useBurstFire = false;
    [Range(0.01f, 0.5f)][SerializeField] float timeBetweenShots = 0.1f; // Delay between shots in a burst
    [SerializeField] int numTurrets;

    int maxHP;
    Color colorOrigin;
    float shootTimer;
    Vector3 playerDir;
    float angleToPlayer;
    bool playerInTrigger = false;
    bool baseInTrigger = false;

    private WaveSpawner waveSpawner;
    private List<Collider> turretsInRange = new List<Collider>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrigin = model.material.color; 
        agent.updateRotation = false;
        gameManager.instance.updateEnemyCount(1);
        gameManager.instance.updateEnemyCountTotal(1);
        waveSpawner = GetComponentInParent<WaveSpawner>();
        maxHP = HP;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
        }
        else if (other.CompareTag("Turret"))
        {
            turretsInRange.Add(other);
        }
        else if (other.CompareTag("Base"))
        {
            baseInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
        }
        else if (other.CompareTag("Turret"))
        {
            turretsInRange.Remove(other);
        }
        else if (other.CompareTag("Base"))
        {
            baseInTrigger = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        shootTimer += Time.deltaTime;
        Debug.DrawRay(transform.position, transform.forward * shootDist, Color.blue);

        // Clean up list
        turretsInRange.RemoveAll(t => t == null);
        numTurrets = turretsInRange.Count;

        // PRIORITY 1: PLAYER (Check CanSeePlayer first as it handles its own movement/shooting)
        if (playerInTrigger && CanSeePlayer())
        {
            // Logic handled inside CanSeePlayer()
        }
        else
        {
            // Default target is the Base
            target = gameManager.instance.baseTower;

            if (target == null)
            {
                agent.isStopped = true;
                return;
            }
            agent.SetDestination(target.transform.position);

            // PRIORITY 2: BASE (Check if base is in range before turrets)
            if (baseInTrigger && target != null)
            {
                // If the base is right here, focus it
                faceTarget(target.GetComponent<Collider>());

                if (shootTimer >= shootRate)
                {
                    Shoot();
                }
            }
            // PRIORITY 3: TURRETS (Only shoot turrets if base is NOT in range)
            else if (turretsInRange.Count > 0)
            {
                faceTarget(turretsInRange[0]);

                if (shootTimer >= shootRate)
                {
                    Shoot();
                }
            }
            // PRIORITY 4: MOVEMENT (Just walk toward base if nothing is in range)
            else
            {
                Vector3 moveDirection = agent.steeringTarget - transform.position;
                moveDirection.y = 0;

                if (moveDirection.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * agent.angularSpeed);
                }
            }
        }
    }

    bool CanSeePlayer()
    {
        playerDir = (gameManager.instance.player.transform.position - headPos.position);
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        RaycastHit hit;
        if (Physics.Raycast(headPos.position, playerDir, out hit))
        {
            if (angleToPlayer <= FOV && hit.collider.CompareTag("Player"))
            {
                target = gameManager.instance.player;
                agent.SetDestination(target.transform.position);

                faceTarget(target.transform.GetComponent<Collider>());

                if (shootTimer >= shootRate)
                {
                    Shoot();
                }

                return true;
            }

        }

        return false;
    }

    void faceTarget(Collider other)
    {
        // 1. Calculate the base direction to the target's center
        Vector3 targetCenter = other.bounds.center;
        Vector3 fullDirection = targetCenter - transform.position;

        if (fullDirection.sqrMagnitude > 0.01f)
        {
            // --- HORIZONTAL ROTATION (Main Model) ---
            // Flatten the direction by removing the Y difference
            Vector3 horizontalDirection = new Vector3(fullDirection.x, 0, fullDirection.z);
            if (horizontalDirection != Vector3.zero)
            {
                Quaternion horizontalRot = Quaternion.LookRotation(horizontalDirection);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, horizontalRot, Time.deltaTime * faceTargetSpeed);
            }

            // --- VERTICAL ROTATION (shootPos) ---
            if (shootPos != null)
            {
                // Direction from shootPos specifically to the target
                Vector3 relativeDir = targetCenter - shootPos.position;

                // LookRotation towards the target, but keep shootPos upright relative to parent
                Quaternion verticalRot = Quaternion.LookRotation(relativeDir);

                // Smoothly rotate the shootPos
                shootPos.rotation = Quaternion.RotateTowards(shootPos.rotation, verticalRot, Time.deltaTime * faceTargetSpeed);

                Debug.DrawRay(shootPos.position, shootPos.forward * shootDist, Color.red);
            }
        }
    }

    void Shoot()
    {
        if (useBurstFire)
        {
            // If burst fire is enabled for this enemy, start the coroutine
            StartCoroutine(FireBurstRoutine());
        }
        else
        {
            // If not using burst fire, fire a single shot exactly as before
            shootTimer = 0; // Reset the timer immediately for the next single shot
            FireProjectile();
        }
    }

    IEnumerator FireBurstRoutine()
    {
        shootTimer = 0; // Reset timer here to define delay between bursts

        for (int i = 0; i < 3; i++)
        {
            FireProjectile(); // Call the helper method to fire the shot

            if (i < 2)
            {
                yield return new WaitForSeconds(timeBetweenShots);
            }
        }
    }


    // Helper method to handle the actual instantiation of the bullet
    void FireProjectile()
    {
        GameObject bulletInstance = Instantiate(bullet, shootPos.position, shootPos.rotation);
        damage bulletDamageScript = bulletInstance.GetComponent<damage>();

        if (bulletDamageScript != null)
        {
            bulletDamageScript.target = gameManager.instance.player.transform;
        }

        foreach (var trail in bulletInstance.GetComponentsInChildren<TrailRenderer>())
        {
            trail.Clear();
        }
    }

    public void takeDamage(int amount, DamageType type)
    {
        HP -= amount;

        if(HP <= 0) 
        {
            gameManager.instance.updateEnemyCount(-1);
            waveSpawner.waves[waveSpawner.currentWaveIndex].enemiesLeft--;
            gameManager.instance.addGold(maxHP);
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(flashRed());
        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrigin;
    }
}
