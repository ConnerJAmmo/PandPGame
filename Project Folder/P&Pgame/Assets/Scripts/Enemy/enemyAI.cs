using bullet.fx.pack;
using System;
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
    [SerializeField] public bool playerDetected;

    [Header("Components")]
    [SerializeField] public float animTranSpeed;
    [SerializeField] public Animator anim;
    [SerializeField] public Transform shootPos;
    [SerializeField] public Transform headPos;
    [SerializeField] public GameObject bullet;
    [SerializeField] public Material texture;

    [Header("Stats")]
    [Range(1, 50)] [SerializeField] public int HP;
    [Range(0, 360)] [SerializeField] public int FOV;
    [Range(0,10)] [SerializeField] float persistenceTime; // Seconds to remember player
    float currentPersistence;
    bool isPlayerInSight;

    [Header("Attack Settings")]
    [SerializeField] bool hasShoot;
    [Range(1, 1000)][SerializeField] public int range;
    [Range(1, 5)][SerializeField] public int shotsPerBurst;
    [Range(0f, 5f)][SerializeField] public float burstFireRate;
    [Range(0.1f, 5f)][SerializeField] float rangedAttackRate;
    [SerializeField] bool hasMelee;
    [Range(0, 10)][SerializeField] float meleeRange;
    [Range(0, 100)][SerializeField] int meleeDamage;
    [Range(0.1f, 5f)][SerializeField] float meleeAttackRate;
    [SerializeField] bool baseFocus;

    [Header("---------Audio---------")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip shootAud;
    [Range(0, 1)] [SerializeField] float shootAudVol;

    private bool playerInTrigger;
    private bool baseInTrigger;
    private bool useBurstFire;
    private int maxHP;
    private float attackTimer;
    private float angleToPlayer;
    private Color colorOrigin;
    private Vector3 playerDir;
    private Material dynamicMat;
    private WaveSpawner waveSpawner;
    private List<Collider> turretsInRange = new List<Collider>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dynamicMat = GetComponentInChildren<Renderer>().material;
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
        //Debug.Log("Checking Tag of Trigger");
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            playerDetected = true;
            //Debug.Log("Player Detected");
        }
        else if (other.CompareTag("Shield"))
        {
            turretsInRange.Add(other);
            //Debug.Log("Shield Detected");
        }
        else if (other.CompareTag("Base"))
        {
            baseInTrigger = true;
            //Debug.Log("Base Detected");
        }
        //else
            //Debug.Log("No Tag Detected");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            playerDetected = false;
            //Debug.Log("Player Exit Trigger");
        }
        else if (other.CompareTag("Shield"))
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
        attackTimer += Time.deltaTime;

        // 1. CLEANUP & SENSING
        turretsInRange.RemoveAll(t => t == null);
        numTurrets = turretsInRange.Count;
        isPlayerInSight = CanSeePlayer();

        // 2. HEALTH CHECK
        if (HP <= 0)
        {
            gameManager.instance.updateEnemyCount(-1);
            waveSpawner.waves[waveSpawner.currentWaveIndex].enemiesLeft--;
            gameManager.instance.addGold(maxHP);
            Destroy(gameObject);
            return; // Exit early to prevent logic running on dead enemy
        }

        // 3. PERSISTENCE LOGIC
        isPlayerInSight = CanSeePlayer(); // This must be the only way to "start" seeing

        if (isPlayerInSight && !baseFocus)
        {
            currentPersistence = persistenceTime;
        }
        else if (playerInTrigger && currentPersistence > 0 && !baseFocus)
        {
            // They only stay "interested" if they already saw you or you're touching them
            currentPersistence -= Time.deltaTime;
        }
        else
        {
            currentPersistence -= Time.deltaTime;
        }

        // 4. PRIORITY DECISION TREE
        // PRIORITY 1: PLAYER (Sticky persistence)
        if (currentPersistence > 0 && !baseFocus)
        {
            target = gameManager.instance.player;
            GetComponent<NavMeshAgent>().SetDestination(target.transform.position); // Set destination here
            TrackAndAttack();
        }
        // PRIORITY 2: TURRETS (If player is gone, check for turrets)
        else if (turretsInRange.Count > 0 && !baseFocus)
        {
            target = turretsInRange[0].gameObject;
            GetComponent<NavMeshAgent>().SetDestination(target.transform.position); // Set destination here
            TrackAndAttack();
        }
        // PRIORITY 3: BASE (Default target)
        else
        {
            target = gameManager.instance.baseTower;
            if (target != null)
            {
                // Remove agent.isStopped = false; it's handled in TrackAndAttack movement logic
                GetComponent<NavMeshAgent>().SetDestination(target.transform.position); // Set destination here

                if (baseInTrigger)
                {
                    TrackAndAttack();
                }
                else
                {
                    // The movement/rotation logic here can stay for smooth base approach
                    Vector3 moveDirection = GetComponent<NavMeshAgent>().steeringTarget - transform.position;
                    moveDirection.y = 0;
                    if (moveDirection.magnitude > 0.1f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * GetComponent<NavMeshAgent>().angularSpeed);
                    }
                }
            }
            else
            {
                GetComponent<NavMeshAgent>().isStopped = true;
            }
        }
    }

    void locoAnim()
    {
        //Debug.Log("Animating");
        float agentSpeedCur = GetComponent<NavMeshAgent>().velocity.normalized.magnitude;
        float agentSpeedAnim = anim.GetFloat("Speed");
        //Debug.Log("Speed: " + agentSpeedCur + " Anim Speed: " + agentSpeedAnim);

        anim.SetFloat("Speed", Mathf.MoveTowards(agentSpeedAnim, agentSpeedCur, Time.deltaTime * animTranSpeed));
    }

    bool CanSeePlayer()
    {
        // Use headPos for the direction and origin
        Vector3 playerDir = (gameManager.instance.player.transform.position - headPos.position).normalized;

        // Check against the HEAD'S forward direction, not the body's
        float angleToPlayer = Vector3.Angle(playerDir, headPos.forward);

        // DOT PRODUCT CHECK (using headPos.forward)
        float dotProduct = Vector3.Dot(playerDir, headPos.forward);

        if (dotProduct < 0 || angleToPlayer > FOV)
        {
            return false;
        }

        // RAYCAST CHECK
        RaycastHit hit;
        if (Physics.Raycast(headPos.position, playerDir, out hit, range))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    void TrackAndAttack()
    {
        Vector3 closestPoint = target.GetComponent<Collider>().ClosestPoint(transform.position);
        float dist = Vector3.Distance(transform.position, closestPoint);

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        Animator anim = GetComponent<Animator>();
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        faceTarget(target.GetComponent<Collider>());

        // 1. MOVEMENT LOGIC
        if (hasMelee && dist <= meleeRange)
            agent.isStopped = true;
        else
            agent.isStopped = false;

        // 2. COMBAT LOGIC
        if (hasMelee && dist <= meleeRange)
        {
            if (attackTimer >= meleeAttackRate)
            {
                attackTimer = 0;
                anim.SetTrigger("Melee");
                // Debug.Log("Melee Triggered");
            }
        }
        else if (hasShoot && dist <= range) // ONLY shoot if not in melee range
        {
            if (attackTimer >= rangedAttackRate)
            {
                attackTimer = 0;
                anim.SetTrigger("Shoot");
            }
        }
    }

    void faceTarget(Collider other)
    {
        if (other == null) return;

        // 1. Get the target's center for precision
        Vector3 targetCenter = other.bounds.center;
        Vector3 directionToTarget = targetCenter - transform.position;

        if (directionToTarget.sqrMagnitude > 0.01f)
        {
            // --- HORIZONTAL ROTATION (Body) ---
            // We only want the body to rotate on the Y-axis (left/right)
            Vector3 horizontalDir = new Vector3(directionToTarget.x, 0, directionToTarget.z);
            if (horizontalDir != Vector3.zero)
            {
                Quaternion bodyRotation = Quaternion.LookRotation(horizontalDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, bodyRotation, Time.deltaTime * 5f);
            }

            // --- VERTICAL ROTATION (Head/ShootPos) ---
            if (headPos != null)
            {
                // Calculate direction from the head to the target
                Vector3 verticalDir = targetCenter - headPos.position;

                if (verticalDir != Vector3.zero)
                {
                    // Create a rotation that looks at the target
                    Quaternion lookAtTarget = Quaternion.LookRotation(verticalDir);

                    // Slerp the head/shootPos independently to look up or down
                    // Adjust '10f' to change how "snappy" the head tracking is
                    headPos.rotation = Quaternion.Slerp(headPos.rotation, lookAtTarget, Time.deltaTime * 10f);

                    // Ensure shootPos stays aligned with head tracking
                    if (shootPos != null)
                    {
                        shootPos.rotation = headPos.rotation;
                    }
                }
            }
        }
    }

    public void Melee()
    {
        if (target != null)
        {
            Vector3 closestPoint = target.GetComponent<Collider>().ClosestPoint(transform.position);
            if (Vector3.Distance(transform.position, closestPoint) <= meleeRange)
            {
                IDamage hit = target.GetComponent<IDamage>();
                if (hit != null)
                {
                    hit.takeDamage(meleeDamage, DamageType.stationary);
                }
            }
        }
    }

    public void Shoot()
    {
        // Ensure we are aiming perfectly right now
        AimShootPosAtTarget(target.transform);

        if (useBurstFire)
        {
            // If burst fire is enabled for this enemy, start the coroutine
            StartCoroutine(FireBurstRoutine());
        }
        else
        {
            aud.PlayOneShot(shootAud);
            FireProjectile(target.transform);
        }
    }

    IEnumerator FireBurstRoutine()
    {
        for (int i = 0; i < shotsPerBurst; i++)
        {
            // Aim immediately before each shot within the burst
            AimShootPosAtTarget(target.transform);

            FireProjectile(target.transform);
            if (i < 2)
            {
                yield return new WaitForSeconds(burstFireRate);
            }
        }
    }

    void AimShootPosAtTarget(Transform targetTransform)
    {
        if (shootPos == null || targetTransform == null) return;

        // Calculate the direction from the gun barrel position to the target's center
        Vector3 targetCenter = targetTransform.GetComponent<Collider>().bounds.center;
        Vector3 directionToTarget = targetCenter - shootPos.position;

        // Create the rotation needed to look along that direction
        Quaternion requiredRotation = Quaternion.LookRotation(directionToTarget);

        // Apply the rotation instantly to override the animation's influence
        shootPos.rotation = requiredRotation;

        // Optional: Draw a debug ray right before the shot fires to verify aim
        Debug.DrawRay(shootPos.position, shootPos.forward * range, Color.green, 1f);
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
        StartCoroutine(flashRed());
    }

    IEnumerator flashRed()
    {
        dynamicMat.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        dynamicMat.color = colorOrigin;
    }
}
