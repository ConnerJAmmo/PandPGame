using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class turretDmg : MonoBehaviour, IDamage
{
    [Header("Stats")]
    [SerializeField] Renderer model;
    [SerializeField] int numEnemies;
    [Range(1, 1000)][SerializeField] int HP = 1000;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform turret;
    [SerializeField] GameObject bullet;
    [SerializeField] LayerMask ignoreLayer;

    [Range(0, 5)][SerializeField] float shootRate;
    [Range(1, 1000)][SerializeField] int shootDist;

    [Header("Idle Scan Settings")]
    [SerializeField] float scanSpeed = 0.5f;
    [SerializeField] float scanAngle = 45f;

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
        enemiesInRange.RemoveAll(enemy => enemy == null);
        numEnemies = enemiesInRange.Count;

        if (enemiesInRange.Count > 0)
        {
            faceTarget();

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

    void faceTarget()
    {
        target = enemiesInRange[0];
        Vector3 centerOfMass = target.GetComponent<Collider>().bounds.center;
        Vector3 direction = centerOfMass - turret.position;
        Quaternion targetRot = Quaternion.LookRotation(direction);
        turret.rotation = Quaternion.RotateTowards(turret.rotation, targetRot, Time.deltaTime * 60);
    }

    void Shoot()
    {
        shootTimer = 0;
        Instantiate(bullet, shootPos.position, turret.rotation);
    }

    
    public void takeDamage(int amount)
    {
        HP -= amount;
        if (HP <= 0)
        {
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
    
}
