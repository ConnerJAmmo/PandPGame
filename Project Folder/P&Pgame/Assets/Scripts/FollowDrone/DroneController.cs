using System;
using System.Collections;
using UnityEngine;


public class DroneController : MonoBehaviour
{
    public bool IsRecruited {  get; private set; }

    [Header("Follow")]
    [SerializeField] float hoverHeight = 6f;
    [SerializeField] Vector3 followOffset = new Vector3(0, 0, -3);
    [SerializeField] float followSmooth = 6f;

    [Header("Takeoff")]
    [SerializeField] float takeoffDuration = 1.5f;
    [SerializeField] float takeoffExtraHeight = 4f;

    [Header("Combat Detect")]
    [SerializeField] float enemyDetectRadius = 22f;
    [SerializeField] LayerMask enemyMask;

    [Header("Shooting")]
    [SerializeField] Transform muzzle;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float fireRate = 18f;
    [SerializeField] float bulletSpeed = 90f;
    [SerializeField] float aimMissRadius = 1.6f;
    [SerializeField] float aimJitter = 0.35f;
    [SerializeField] float maxShootRange = 45f;
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip audClip;
    [Range(0,1)][SerializeField] float audVol;

    [Header("Bank / Tilt")]
    [SerializeField] Transform droneRoot;
    [SerializeField] float maxBankAngle = 18f;
    [SerializeField] float maxPitchAngle = 10f;
    [SerializeField] float bankSmooth = 8f;

    [SerializeField] KeyCode toggleFireKey = KeyCode.T;
    [SerializeField] bool shootingEnabled = true;

    [Header("UI optional")]
    [SerializeField] DroneWorldUI ui; // my world space UI script

    Transform player;
    float fireTimer;
    bool takingOff;

    Vector3 lastPos;
    Vector3 vel;
    Quaternion modelBaseRot;

    private void Start()
    {
        lastPos = transform.position;
        if (droneRoot) modelBaseRot = droneRoot.localRotation;
    }

    private void Update()
    {
        if (!IsRecruited || !player) return;

        if (takingOff) return;

        FollowPlayer();
        UpdateBanking();

        if (Input.GetKeyDown(toggleFireKey))
        {
            shootingEnabled = !shootingEnabled;
            if (shootingEnabled)
                SetHint("Drone Weapons: ON");
            else
                SetHint("Drone Weapons: OFF");
        }

        

        if (shootingEnabled)
        {
            Transform enemy = FindEnemy();
            if (enemy)
                ShootAround(enemy);
        }
    }

    private void LateUpdate()
    {
        Debug.Log("Drone Pos: " + transform.position);
        if(IsRecruited)
            Debug.DrawLine(transform.position, transform.position + Vector3.up * 3f, Color.cyan);
    }

    public void Recruit(Transform playerTransform)
    {
        if (IsRecruited) return;
        player = playerTransform;
        IsRecruited = true;
        ClearHint();
        StartCoroutine(TakeOffRoutine());
        
    }

    IEnumerator TakeOffRoutine()
    {
        takingOff = true;

        Vector3 start = transform.position;
         //end = player.position + Vector3.up * (player.position.y + hoverHeight + takeoffExtraHeight);

        Vector3 end = player.position + Vector3.up * (hoverHeight + takeoffExtraHeight);

        float t = 0f;
        while (t < takeoffDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / takeoffDuration);

            //ease out and tiny wobble
            float wobble = Mathf.Sin(Time.time * 12f) * 0.08f;
            transform.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0, 1, a)) + Vector3.up * wobble;

            yield return null;

        }

        takingOff = false;
    }

    

    private void UpdateBanking()
    {
        if (!droneRoot) { return; }

        // velocity from actual movement (smooth + stable)
        vel = (transform.position - lastPos) / Mathf.Max(Time.deltaTime, .0001f);
        lastPos = transform.position;

        // convert to local space so is relative drone
        Vector3 localVel = transform.InverseTransformDirection(vel);

        // normalize using a feel speed so tiny movement don't over-bank
        float feelSpeed = 10f; //tweak as I need it
        float side = Math.Clamp(localVel.x / feelSpeed, -1f, 1f);
        float foward = Mathf.Clamp(localVel.z / feelSpeed, -1f, 1f);

        // Bank left/right roll around z
        float roll = -side * maxBankAngle;

        // Pitch foward/back pitch around x
        float pitch = foward * -maxPitchAngle;

        Quaternion target = modelBaseRot * Quaternion.Euler(pitch, 0f, roll);
        droneRoot.localRotation = Quaternion.Slerp(droneRoot.localRotation, target, Time.deltaTime * bankSmooth);
            
            
    }

    private void FollowPlayer()
    {
        Vector3 target = player.position + player.TransformDirection(followOffset);
        target.y = player.position.y + hoverHeight;

        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * followSmooth);

        // face forward-ish 
        Vector3 flatforward = player.forward; flatforward.y = 0;
        if (flatforward.sqrMagnitude > 0.001f)
        {
            Quaternion look = Quaternion.LookRotation(flatforward.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * 6f);
        }
    }

    Transform FindEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, enemyDetectRadius, enemyMask);
        float val = float.MaxValue;
        Transform chosen = null;

        foreach (Collider coll in hits)
        {
            float d = Vector3.Distance(transform.position, coll.transform.position);
            if (d < val)
            {
                val = d;
                chosen = coll.transform;
            }
        }
        return chosen;
    }

    void ShootAround(Transform enemy)
    {
        if (!muzzle || !bulletPrefab)
            return;

        fireTimer += Time.deltaTime;
        float interval = 1f / Mathf.Max(1f, fireRate);
        if (fireTimer < interval) return;
        fireTimer = 0f;

        // Pick a random point near the enemy instead of "lock - on"
        Vector3 randomOffset = (UnityEngine.Random.insideUnitSphere * aimMissRadius) + (UnityEngine.Random.insideUnitSphere * aimJitter);

        randomOffset.y *= 0.35f; // keep it mostly horizontal

        Vector3 targetPoint = enemy.position + randomOffset;

        // Aim direction
        Vector3 dir = (targetPoint - muzzle.position);
        float dist = dir.magnitude;
        if (dist > maxShootRange)
            return;

        dir /= Mathf.Max(0.001f, dist);

        // Spawn bullet

        var go = Instantiate(bulletPrefab, muzzle.position, Quaternion.LookRotation(dir));
        var b = go.GetComponent<DroneBullet>();
        if (b)
        {
            b.Init(dir, bulletSpeed);
            if (aud && audClip)
                aud.PlayOneShot(audClip, audVol);
        }
        else
        {
            //fallback if no script
            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb) rb.linearVelocity = dir * bulletSpeed;
        }
    }

    public void SetProgress(float t)
    {
        if (ui) ui.SetProgress(t);
    }

    public void SetHint(string msg)
    {
        if (ui) ui.SetHint(msg);
    }

    public void ClearHint()
    {
        if (ui) ui.ClearHint();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyDetectRadius);
    }
}
