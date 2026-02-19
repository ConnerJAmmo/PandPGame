using bullet.fx.pack;
using System;
using UnityEngine;

public class ShipTurretController : MonoBehaviour
{
    [Header("Parts")]
    [SerializeField] Transform baseYaw;
    [SerializeField] Transform barrelPitch;
    [SerializeField] Transform muzzle;

    [Header("Rotation")]
    [SerializeField] float turnSpeed;
    [SerializeField] float pitchSpeed;
    [SerializeField] float minPitch;
    [SerializeField] float maxPitch;

    [Header("Combat")]
    [SerializeField] float detectRadius = 35f;
    [SerializeField] LayerMask enemyMask;
    //[SerializeField] GameObject bulletPrefab;
    [SerializeField] float fireRate = 8;
    [SerializeField] float bulletSpeed = 80;

    [Header("Damage")]
    [SerializeField] int damage = 10;
    [SerializeField] float hitRange = 80f;
    [SerializeField] DamageType damageType = DamageType.moving;
    [SerializeField] LayerMask hitMask = ~0;
    [SerializeField] GameObject hitVfxEnemy;
    [SerializeField] GameObject hitVfxGround;

    float fireTimer;
    Transform currentTarget;

    public bool IsActive { get; private set; }

    public void Activate()
    {
        IsActive = true;
        fireTimer = 0;
    }

    private void Update()
    {
        if (!IsActive) return;

        FindTarget();

        if (!currentTarget) return;

        AimAtTarget();
        Shoot();
    }

    private void Shoot()
    {
        fireTimer += Time.deltaTime;

        if (fireTimer < 1f / fireRate)
            return;

        fireTimer = 0f;

        if (Physics.Raycast(muzzle.position, muzzle.forward, out RaycastHit hit, hitRange, hitMask, QueryTriggerInteraction.Ignore))
        {
            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg == null) 
                dmg = hit.collider.GetComponentInParent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(damage, damageType);
                if (hitVfxEnemy)
                    Instantiate(hitVfxEnemy, hit.point, Quaternion.LookRotation(hit.normal));
            }
            else
            {
                if (hitVfxGround)
                    Instantiate(hitVfxGround, hit.point, Quaternion.LookRotation(hit.normal));
            }
                    
        }
       
    }

    private void AimAtTarget()
    {
        Vector3 dir = currentTarget.position - baseYaw.position;
        dir.y = 0;

        Quaternion yawRot = Quaternion.LookRotation(dir);
        baseYaw.rotation = Quaternion.RotateTowards(baseYaw.rotation, yawRot, turnSpeed * Time.deltaTime);

        Vector3 localDir = barrelPitch.InverseTransformPoint(currentTarget.position);
        float angle = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;
        angle = Mathf.Clamp(angle, minPitch, maxPitch);

        Quaternion pitchRot = Quaternion.Euler(angle, 0, 0);
        barrelPitch.localRotation = Quaternion.RotateTowards(barrelPitch.localRotation, pitchRot, pitchSpeed * Time.deltaTime);
    }

    private void FindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectRadius, enemyMask);

        float val = float.MaxValue;
        Transform chosen = null;

        foreach (var hit in hits)
        {
            IDamage dmg = hit.GetComponentInParent<IDamage>();
            if(dmg == null) continue;

            float d = Vector3.Distance(transform.position, hit.transform.position);
            if (d < val)
            {
                val = d;
                chosen = hit.transform;
            }
        }

        currentTarget = chosen;
    }
}
