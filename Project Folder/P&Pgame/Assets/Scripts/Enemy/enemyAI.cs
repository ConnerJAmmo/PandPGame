using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class enemyAI : MonoBehaviour, IDamage
{
    [Header("Components")]
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Vector3 desination;
    [SerializeField] Transform target;
    [SerializeField] Transform shootPos;
    [SerializeField] GameObject bullet;
    [SerializeField] LayerMask ignoreLayer;

    [Header("Stats")]
    [Range(1, 25)] [SerializeField] int HP;
    [Range(0, 2)] [SerializeField] float shootRate;
    [Range(1, 1000)] [SerializeField] int faceTargetSpeed;
    [Range(1, 1000)][SerializeField] int shootDist;

    Color colorOrigin;
    float shootTimer;
    bool targetAquired = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrigin = model.material.color; 
        agent.updateRotation = false;
        gameManager.instance.updateGameGoal(1);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            targetAquired = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            targetAquired = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        shootTimer += Time.deltaTime;

        desination = gameManager.instance.baseTower.transform.position;
        agent.SetDestination(desination);

        Debug.DrawRay(transform.position, transform.forward * shootDist, Color.blue);

        if (targetAquired)
        {
            faceTarget();

            if (shootTimer >= shootRate)
            {
                Shoot();
            }
        }
        else
        {
            Vector3 moveDirection = agent.steeringTarget - transform.position;
            moveDirection.y = 0; // Keep the agent upright

            if (moveDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                // Smoothly rotate towards the movement direction
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * agent.angularSpeed);
            }
        }
    }

    void faceTarget()
    {
        // 1. Calculate the base direction to the target's center
        Vector3 targetCenter = gameManager.instance.player.transform.GetComponent<Collider>().bounds.center;
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

    /*
    void faceTarget()
    {
        Vector3 centerOfMass = gameManager.instance.player.transform.GetComponent<Collider>().bounds.center;
        Vector3 direction = centerOfMass - transform.position;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, Time.deltaTime * faceTargetSpeed);
        }
    }
    */

    void Shoot()
    {
        shootTimer = 0;
        Instantiate(bullet, shootPos.position, shootPos.rotation);
        foreach (var trail in bullet.GetComponentsInChildren<TrailRenderer>())
        {
            trail.Clear();
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        if(HP <= 0) 
        {
            gameManager.instance.updateGameGoal(-1);
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
