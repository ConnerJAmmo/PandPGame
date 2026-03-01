using bullet.fx.pack;
using System;
using System.Collections;
using UnityEngine;

public class ShipTurretController : MonoBehaviour
{
    [Header("Parts")]
    [SerializeField] Transform baseYaw;
    [SerializeField] Transform barrelPitch;
    [SerializeField] Transform muzzle;

    [Header("Rotation")]
    [SerializeField] float turnSpeed = 180;
    [SerializeField] float pitchSpeed = 140;
    [SerializeField] float minPitch;
    [SerializeField] float maxPitch;

    [Header("Combat")]
    [SerializeField] float detectRadius = 35f;
    [SerializeField] LayerMask enemyMask;
    [SerializeField] float fireRate = 8;
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

    [Header("Burst")]
    [SerializeField] int burstCount = 3;
    [SerializeField] float burstShotInterval = 0.06f; // controls how fast the 3 round burst

    [Header("Heat / Overheat")]
    [SerializeField] float heatPerShot = 0.12f;   // heat gained per bullet
    [SerializeField] float coolPerSecond = 0.25f; //cooling rate
    [SerializeField] float overHeatAt = 1.0f;     //threshold
    [SerializeField] float resumeAt = 0.35f;      // must cool below this to fire

    [Header("Heat UI")]
    [SerializeField] GameObject heatBarPrefab;
    [SerializeField] Vector3 heatBarOffset = new Vector3(0, 5f, 0);
    TurretHeatUI heatUI;
    Transform heatBarRoot;

    float fireTimer;
    Transform currentTarget;

    float heat01 = 0f;
    bool overheated;
    bool burstRunning;

    public bool IsActive { get; private set; }

 

    private void Start()
    {
        //Activate();
        //Spawn heat UI
        if(heatBarPrefab)
        {
            var go = Instantiate(heatBarPrefab, transform);
            heatBarRoot = go.transform;
            heatBarRoot.localPosition = heatBarOffset;
            heatUI = go.GetComponentInChildren<TurretHeatUI>(true);

        }
    }

    private void Update()
    {
        if (!IsActive) return;

        // Passive cooling
        CoolDownHeat();

        FindTarget();
        if (!currentTarget) return;

        AimAtTarget();

        if (overheated) return;
        if (burstRunning) return;

        fireTimer += Time.deltaTime;
        float burstInterval = 1f / Mathf.Max(0.01f, fireRate); //burst per second
        if (fireTimer >= burstInterval)
        {
            fireTimer = 0f;
            StartCoroutine(BurstRoutine());
        }
        // Shoot();
    }

    public void Activate()
    {
        IsActive = true;
        fireTimer = 0;
    }

    IEnumerator BurstRoutine()
    {
        burstRunning = true;

        for(int i = 0; i < burstCount; i++)
        {
            // if target disappears mid-burst, stop
            if (!currentTarget) break;

            FireOneShot();

            //heat check
            heat01 += heatPerShot;
            if (heat01 >= overHeatAt)
            {
                heat01 = overHeatAt;
                overheated = true;
                UpdateHeatUI();
                break;
            }
            UpdateHeatUI();
            yield return new WaitForSeconds(burstShotInterval);
        }

        burstRunning = false;
    }

    private void UpdateHeatUI()
    {
        if (heatUI) heatUI.SetHeat01(heat01);
    }

    private void FireOneShot()
    {
        if (!muzzle || !currentTarget) return;

        // muzzleFlash + sound
        
        if (aud && shotSfx) aud.PlayOneShot(shotSfx, shotVol);

        Vector3 aimPoint = currentTarget.position + Vector3.up * aimHeight;

        Vector3 start = muzzle.position;
        Vector3 dir = (aimPoint - start).normalized;

        int combineMask = enemyMask.value | blockMask.value;

        Vector3 end = start + dir * hitRange;

        if (Physics.Raycast(start, dir, out RaycastHit hit, hitRange, combineMask, QueryTriggerInteraction.Collide))
        {
            end = hit.point;
            if (muzzleFlash) muzzleFlash.Play();
            Debug.Log("Hit: " + hit.collider.name);

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
            var tr = Instantiate(tracerPrefab, start, Quaternion.identity);
            tr.useWorldSpace = true;
            tr.positionCount = 2;
            tr.SetPosition(0, start);
            tr.SetPosition(1, end);
            Destroy(tr.gameObject, tracerLife);
        }
    }

    private void CoolDownHeat()
    {
        if (heat01 <= 0f) return;

        heat01 = Mathf.Max(0f, heat01 - coolPerSecond * Time.deltaTime);

        if (overheated && heat01 <= resumeAt)
            overheated = false;

        UpdateHeatUI();
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
        Collider[] hits = Physics.OverlapSphere(transform.position, detectRadius, enemyMask, QueryTriggerInteraction.Collide);

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
        Debug.Log("Enemies detected: " + hits.Length);
        currentTarget = chosen;
    }
}
