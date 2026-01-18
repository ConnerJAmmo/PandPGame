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

        Debug.DrawRay(shootPos.position, transform.forward * shootDist, Color.red);

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
        Vector3 centerOfMass = gameManager.instance.player.transform.GetComponent<Collider>().bounds.center;
        Vector3 direction = centerOfMass - transform.position;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, Time.deltaTime * faceTargetSpeed);
        }
    }
    
    void Shoot()
    {
        shootTimer = 0;
        Instantiate(bullet, shootPos.position, transform.rotation);
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
