using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [Header("Components")]
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform shootPos;
    [SerializeField] GameObject bullet;
    [SerializeField] LayerMask ignoreLayer;

    [Header("Stats")]
    [Range(1, 10)] [SerializeField] int HP;
    [Range(0, 2)] [SerializeField] float shootRate;
    [Range(1, 10)] [SerializeField] int faceTargetSpeed;
    [Range(1, 1000)][SerializeField] int shootDist;

    Color colorOrigin;
    float shootTimer;
    Vector3 playerDir;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrigin = model.material.color;
        gameManager.instance.updateGameGoal(1);
    }

    // Update is called once per frame
    void Update()
    {
        shootTimer += Time.deltaTime;

        playerDir = (gameManager.instance.player.transform.position - transform.position);

        agent.SetDestination(gameManager.instance.baseTower.transform.position);

        Debug.DrawRay(shootPos.position, transform.forward * shootDist, Color.red);

        faceTarget();

        RaycastHit hit;

        if (Physics.Raycast(shootPos.position, transform.forward, out hit, shootDist, ~ignoreLayer) && shootTimer >= shootRate)
        {
            if (hit.collider.CompareTag("Player"))
            {
                Shoot();
            }
        }
    }

    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, transform.position.y, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }
    
    void Shoot()
    {
        shootTimer = 0;
        Instantiate(bullet, shootPos.position, transform.rotation);
    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        if(HP < 0) 
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
