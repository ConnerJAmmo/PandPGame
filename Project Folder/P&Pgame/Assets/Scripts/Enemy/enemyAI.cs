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
    [Range(1, 25)] [SerializeField] public int HP;
    [Range(0, 360)] [SerializeField] public int FOV;
    [Range(0,10)] [SerializeField] float persistenceTime; // Seconds to remember player
    float currentPersistence;
    bool isPlayerInSight;
    [SerializeField] bool hasMelee;
    [Range(0, 10)][SerializeField] float meleeRange;
    [Range(0, 100)][SerializeField] int meleeDamage;

    [Header("---------Audio---------")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip shootAud;
    [Range(0, 1)] [SerializeField] float shootAudVol;

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
        shootTimer += Time.deltaTime;

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
        if (isPlayerInSight && playerInTrigger)
            currentPersistence = persistenceTime;
        else
            currentPersistence -= Time.deltaTime;

        // 4. PRIORITY DECISION TREE
        // PRIORITY 1: PLAYER (Sticky persistence)
        if (currentPersistence > 0)
        {
            TrackAndAttack();
        }
        // PRIORITY 2: TURRETS (If player is gone, check for turrets)
        else if (turretsInRange.Count > 0)
        {
            target = turretsInRange[0].gameObject;
            GetComponent<NavMeshAgent>().SetDestination(target.transform.position);
            faceTarget(turretsInRange[0]);

            TrackAndAttack();
        }
        // PRIORITY 3: BASE (Default target)
        else
        {
            target = gameManager.instance.baseTower;

            if (target != null)
            {
                GetComponent<NavMeshAgent>().isStopped = false;
                GetComponent<NavMeshAgent>().SetDestination(target.transform.position);

                if (baseInTrigger)
                {
                    faceTarget(target.GetComponent<Collider>());
                    TrackAndAttack();
                }
                else
                {
                    // Smooth movement rotation toward base
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
        Vector3 playerDir = (gameManager.instance.player.transform.position - headPos.position);
        float angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        // 1. FOV CHECK FIRST (The "Eyes" check)
        if (angleToPlayer > FOV)
        {
            return false;
        }

        // 2. RAYCAST CHECK SECOND (The "Wall" check)
        RaycastHit hit;
        if (Physics.Raycast(headPos.position, playerDir, out hit))
        {
            // Only return true if the first thing we hit is actually the player
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    void TrackAndAttack()
    {
        target = gameManager.instance.player;
        float dist = Vector3.Distance(transform.position, target.transform.position);
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        Animator anim = GetComponent<Animator>();
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        faceTarget(target.GetComponent<Collider>());

        // 1. MOVEMENT LOGIC
        if (hasMelee && dist <= meleeRange)
            agent.isStopped = true;
        else
        {
            agent.isStopped = false;
            agent.SetDestination(target.transform.position);
        }

        // 2. COMBAT LOGIC
        if (shootTimer >= fireRate)
        {
            if (hasMelee && dist <= meleeRange)
            {
                // Reset the timer IMMEDIATELY when the decision to attack is made
                shootTimer = 0;

                // Only fire the trigger if the animator isn't already busy punching
                if (!stateInfo.IsName("Monster01_Attack03_InPlace") && !anim.IsInTransition(0))
                {
                    anim.SetTrigger("Melee");

                    StartCoroutine(ClearTriggerAfterFrame("Melee"));
                }
            }
            else if (!hasMelee || dist > meleeRange)
            {
                shootTimer = 0;
                anim.ResetTrigger("Melee");
                Shoot();
            }
        }
    }

    IEnumerator ClearTriggerAfterFrame(string triggerName)
    {
        yield return null; // Wait one frame
        GetComponent<Animator>().ResetTrigger(triggerName);
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

    public void MeleeImpact()
    {
        // Re-check distance to ensure player didn't dodge mid-animation
        if (target != null && Vector3.Distance(transform.position, target.transform.position) <= meleeRange)
        {
            IDamage hit = target.GetComponent<IDamage>();
            if (hit != null)
            {
                hit.takeDamage(meleeDamage, DamageType.stationary);
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

            aud.PlayOneShot(shootAud);
            
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
        StartCoroutine(flashRed());
    }

    IEnumerator flashRed()
    {
        dynamicMat.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        dynamicMat.color = colorOrigin;
    }
}
