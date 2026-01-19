using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class turretDmg : MonoBehaviour//, IDamage
{
    [Header("Stats")]
    [SerializeField] Renderer model;
    [SerializeField] int numEnemies;
    [Range(1, 1000)][SerializeField] int HP;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform turret;
    [SerializeField] GameObject bullet;
    [SerializeField] LayerMask ignoreLayer;

    [Range(0, 5)][SerializeField] float shootRate;
    [Range(1, 1000)][SerializeField] int shootDist;

    Color colorOrigin;
    float nextDamageTime;
    float shootTimer;
    [SerializeField] Collider target;
    Quaternion forward;

    private List<Collider> enemiesInRange = new List<Collider>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        forward = Quaternion.LookRotation(turret.transform.forward);
        colorOrigin = model.material.color;
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

        // 1. Clean the list first
        enemiesInRange.RemoveAll(enemy => enemy == null);
        numEnemies = enemiesInRange.Count;

        if (enemiesInRange.Count > 0)
        {
            faceTarget();

            if (shootTimer >= shootRate)
            {
                // Optional: Only shoot if the turret is actually pointing at the target
                Shoot();
            }
        }
        else
        {
            // 2. Rotate the TURRET back to forward, not the whole object
            turret.rotation = Quaternion.Slerp(turret.rotation, forward, Time.deltaTime * 2);
        }
    }


    void faceTarget()
    {
        target = enemiesInRange[0];
        Vector3 centerOfMass = target.GetComponent<Collider>().bounds.center;
        Vector3 direction = centerOfMass - turret.position;
        Quaternion targetRot = Quaternion.LookRotation(direction);
        turret.rotation = Quaternion.RotateTowards(turret.rotation, targetRot, Time.deltaTime * 60);
    }

    /*
    void faceTarget()
    {
        target = enemiesInRange[0];

        Quaternion rot = Quaternion.LookRotation(new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z));
        turret.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }
    */
    void Shoot()
    {
        shootTimer = 0;
        Instantiate(bullet, shootPos.position, turret.rotation);
    }

    /*
    public void takeDamage(int amount)
    {
        HP -= amount;
        if (HP <= 0)
        {
            gameManager.instance.youLose();
            Destroy(gameObject);
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(flashRed());
        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrigin;
    }
    */
}
