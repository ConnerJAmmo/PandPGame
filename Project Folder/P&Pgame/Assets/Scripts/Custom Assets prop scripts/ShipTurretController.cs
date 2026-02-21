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
    [SerializeField] float fireRate = 8;
    [SerializeField] float bulletSpeed = 80;
    [SerializeField] float aimHeight = 1.2f;

    [Header("Damage")]
    [SerializeField] int damage = 10;
    [SerializeField] float hitRange = 80f;
    [SerializeField] DamageType damageType = DamageType.moving;
    [SerializeField] LayerMask blockMask;
    [SerializeField] ParticleSystem muzzleFlash;
    [SerializeField] GameObject hitVfxEnemy;
    [SerializeField] GameObject hitVfxGround;

    [Header("Tracer")]
    [SerializeField] LineRenderer tracerPrefab;
    [SerializeField] float tracerLife = 0.05f;

    [Header("Audio")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip shotSfx;
    [Range(0, 1)][SerializeField] float shotVol = 0.5f;

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

        if (!muzzle || !currentTarget) return;

        // muzzleFlash + sound
        if (muzzleFlash) muzzleFlash.Play();
        if (aud && shotSfx) aud.PlayOneShot(shotSfx, shotVol);

        Vector3 aimPoint = currentTarget.position + Vector3.up * aimHeight;

        Vector3 start = muzzle.position;
        Vector3 dir = (aimPoint - start).normalized;

        int combineMask = enemyMask.value | blockMask.value;

        Vector3 end = start + dir * hitRange;

        if (Physics.Raycast(start, dir, out RaycastHit hit, hitRange, combineMask, QueryTriggerInteraction.Ignore))
        {
            end = hit.point;

            bool hitIsEnemy = (enemyMask.value & (1 << hit.collider.gameObject.layer)) != 0;

            if (hitIsEnemy)
            {
                IDamage dmg = hit.collider.GetComponent<IDamage>();
                if (dmg == null) dmg = hit.collider.GetComponentInParent<IDamage>();

                if (dmg != null)
                {
                    dmg.takeDamage(damage, damageType);

                    if (hitVfxEnemy)
                        Instantiate(hitVfxEnemy, hit.point, Quaternion.LookRotation(hit.normal));
                }
            }
            else
            {
                if (hitVfxGround)
                    Instantiate(hitVfxGround, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }

        if (tracerPrefab)
        {
            var tr = Instantiate(tracerPrefab);
            tr.positionCount = 2;
            tr.SetPosition(0, start);
            tr.SetPosition(1, end);
            Destroy(tr.gameObject, tracerLife);
        }
    }

    private void AimAtTarget()
    {
        if (!currentTarget) return;

        Vector3 aimPoint = currentTarget.position + Vector3.up * aimHeight;

        // Yaw
        Vector3 toTarget = aimPoint - baseYaw.position;
        Vector3 flat = new Vector3(toTarget.x, 0f, toTarget.z);

        if(flat.sqrMagnitude > 0.0001f)
        {
            Quaternion yawTarget = Quaternion.LookRotation(flat.normalized, Vector3.up);
            baseYaw.rotation = Quaternion.RotateTowards(baseYaw.rotation, yawTarget, turnSpeed * Time.deltaTime);
        }

        Vector3 dirWorld = aimPoint - barrelPitch.position;
        Vector3 dirLocal = baseYaw.InverseTransformDirection(dirWorld);

        
        float pitch = Mathf.Atan2(dirLocal.y, dirLocal.z) * Mathf.Rad2Deg;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion pitchTarget = Quaternion.Euler(pitch, 0f, 0f);
        barrelPitch.localRotation = Quaternion.RotateTowards(barrelPitch.localRotation, pitchTarget, pitchSpeed * Time.deltaTime);

        //Debug.DrawLine(muzzle.position, aimPoint, Color.red);
        //Debug.DrawRay(muzzle.position, muzzle.forward * 10f, Color.blue);
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
