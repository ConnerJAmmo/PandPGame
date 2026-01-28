using bullet.fx.pack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class enemyAI : MonoBehaviour, IDamage
{
    [Header("Debug")]
    [SerializeField] public int numTurrets;
    [SerializeField] public GameObject target;

    [Header("Components")]
    [SerializeField] public float animTranSpeed;
    [SerializeField] public Animator anim;
    [SerializeField] public Transform shootPos;
    [SerializeField] public Transform headPos;
    [SerializeField] public GameObject bullet;
    [SerializeField] public Material texture;

    [Header("Stats")]
    [Range(1, 25)] [SerializeField] public int HP;
    [Range(0, 360)] [SerializeField] public int FOV;

    [Header("Fire Settings")]
    [Range(1, 1000)] [SerializeField] public int range;
    [Range(1, 5)] [SerializeField] public int shotsPerBurst;
    [Range(0, 2)] [SerializeField] public float burstFireRate;
    [Range(0.01f, 2)] [SerializeField] public float fireRate;

    private bool playerInTrigger;
    private bool baseInTrigger;
    private bool useBurstFire;
    private int maxHP;
    private float shootTimer;
    private float angleToPlayer;
    private Color colorOrigin;
    private Vector3 playerDir;
    private Material dynamicMat;
    private WaveSpawner waveSpawner;
    private List<Collider> turretsInRange = new List<Collider>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dynamicMat = texture;
        colorOrigin = dynamicMat.color;
        GetComponent<NavMeshAgent>().updateRotation = false;
        gameManager.instance.updateEnemyCount(1);
        gameManager.instance.updateEnemyCountTotal(1);
        waveSpawner = GetComponentInParent<WaveSpawner>();
        maxHP = HP;
        playerInTrigger = false;
        baseInTrigger = false;
        if (shotsPerBurst == 1)
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
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player Detected");
            playerInTrigger = true;
        }
        else if (other.CompareTag("Turret"))
        {
            Debug.Log("Turret Detected");
            turretsInRange.Add(other);
        }
        else if (other.CompareTag("Base"))
        {
            Debug.Log("Base Detected");
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
        locoAnim();

        shootTimer += Time.deltaTime;
        Debug.DrawRay(transform.position, transform.forward * range, Color.blue);

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
                GetComponent<NavMeshAgent>().isStopped = true;
                return;
            }
            GetComponent<NavMeshAgent>().SetDestination(target.transform.position);

            // PRIORITY 2: BASE (Check if base is in range before turrets)
            if (baseInTrigger && target != null)
            {
                // If the base is right here, focus it
                faceTarget(target.GetComponent<Collider>());

                if (shootTimer >= fireRate)
                {
                    Shoot();
                }
            }
            // PRIORITY 3: TURRETS (Only shoot turrets if base is NOT in range)
            else if (turretsInRange.Count > 0)
            {
                faceTarget(turretsInRange[0]);

                if (shootTimer >= fireRate)
                {
                    Shoot();
                }
            }
            // PRIORITY 4: MOVEMENT (Just walk toward base if nothing is in range)
            else
            {
                Vector3 moveDirection = GetComponent<NavMeshAgent>().steeringTarget - transform.position;
                moveDirection.y = 0;

                if (moveDirection.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * GetComponent<NavMeshAgent>().angularSpeed);
                }
            }
        }
    }

    void locoAnim()
    {
        float agentSpeedCur = GetComponent<NavMeshAgent>().velocity.normalized.magnitude;
        float agentSpeedAnim = anim.GetFloat("Speed");

        anim.SetFloat("Speed", Mathf.MoveTowards(agentSpeedAnim, agentSpeedCur, Time.deltaTime * animTranSpeed));
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
                GetComponent<NavMeshAgent>().SetDestination(target.transform.position);

                faceTarget(target.transform.GetComponent<Collider>());
                Debug.Log("Player Seen");

                if (shootTimer >= fireRate)
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
                transform.rotation = Quaternion.RotateTowards(transform.rotation, horizontalRot, Time.deltaTime * 450f);
            }

            // --- VERTICAL ROTATION (shootPos) ---
            if (shootPos != null)
            {
                // Direction from shootPos specifically to the target
                Vector3 relativeDir = targetCenter - shootPos.position;

                // LookRotation towards the target, but keep shootPos upright relative to parent
                Quaternion verticalRot = Quaternion.LookRotation(relativeDir);

                // Smoothly rotate the shootPos
                shootPos.rotation = Quaternion.RotateTowards(shootPos.rotation, verticalRot, Time.deltaTime * 450f);

                Debug.DrawRay(shootPos.position, shootPos.forward * range, Color.red);
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
            anim.SetTrigger("Shoot");
            FireProjectile(target.transform);
        }
    }

    IEnumerator FireBurstRoutine()
    {
        shootTimer = 0; // Reset timer here to define delay between bursts

        for (int i = 0; i < shotsPerBurst; i++)
        {
            FireProjectile(target.transform); // Call the helper method to fire the shot
            anim.SetTrigger("Shoot");
            if (i < 2)
            {
                yield return new WaitForSeconds(burstFireRate);
            }
        }
    }


    // Helper method to handle the actual instantiation of the bullet
    void FireProjectile(Transform currentTarget)
    {
        GameObject bulletInstance = Instantiate(bullet, shootPos.position, shootPos.rotation);
        damage bulletDamageScript = bulletInstance.GetComponent<damage>();

        if (bulletDamageScript != null)
        {
            bulletDamageScript.target = currentTarget;
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
        dynamicMat.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        dynamicMat.color = colorOrigin;
    }
}
