using Unity.VisualScripting;
using UnityEngine;

public class RadarTrackPlayer : MonoBehaviour
{
    [Header("All of the turnable parts")]
    [SerializeField] Transform horizPlatformTurn;
    [SerializeField] Transform tiltPole;
    [SerializeField] Transform dish;

    [Header("Detection")]
    [SerializeField] float detectionRadius;
    [SerializeField] LayerMask playerMask = ~0;

    [Header("Rotation Speeds")]
    [SerializeField] float yawSpeed = 120f;
    [SerializeField] float tiltSpeed = 90f;

    [Header("Tilt limits")]
    [SerializeField] bool enableTilt = true;
    [SerializeField] float minTilt = -15f;
    [SerializeField] float maxTilt = 35f;

    [Header("Idle")]
    [SerializeField] bool IdleSpin = true;
    [SerializeField] float idleYawSpeed = 25f;

    Transform player;
    bool playerInRange;

    void Reset()
    {
        // Add all of the object automatically
        if (!horizPlatformTurn) horizPlatformTurn = transform.Find("horizPlatformTurn");
        if (horizPlatformTurn && !tiltPole)
            tiltPole = horizPlatformTurn.Find("Arms/TiltPole");
        if (tiltPole && !dish)
            dish = tiltPole.Find("Dish");
    }
    void Update()
    {
        FindPlayerIfNeeded();

        if (!horizPlatformTurn)
            return;

        if (playerInRange && player)
        {
            TrackPlayer();
        }
        else if (IdleSpin)
        {
            horizPlatformTurn.Rotate(0f, idleYawSpeed * Time.deltaTime, 0f, Space.Self);
        }
    }

    void FindPlayerIfNeeded()
    {
        if (player)
        {
            float dist = Vector3.Distance(player.position, transform.position);
            playerInRange = dist <= detectionRadius;
            return;
        }


        Collider[] hits= Physics.OverlapSphere(transform.position, detectionRadius, playerMask);
        
        // We only have one player, but this an array of any players (collier components)
        foreach(var h in hits)
        {
            if (h.CompareTag("Player"))
            {
                player = h.transform;
                playerInRange = true;
                return;
            }
        }
        playerInRange = false;
    }

    void TrackPlayer()
    {
        Vector3 toPLayer = player.position - horizPlatformTurn.position;

        // turn the platform
        Vector3 flat = new Vector3(toPLayer.x, 0f, toPLayer.z);
        if (flat.sqrMagnitude > 0.0001f)
        {
            Quaternion targetYaw = Quaternion.LookRotation(flat.normalized, Vector3.up);
            horizPlatformTurn.rotation = Quaternion.RotateTowards(horizPlatformTurn.rotation, targetYaw, yawSpeed * Time.deltaTime);

        }

        //tilt
        if (!enableTilt || !tiltPole)
            return;

        // compare our desired pitch rel to tiltPole parent
        Vector3 localDir = tiltPole.parent.InverseTransformDirection(toPLayer.normalized);


        // Looking up and down
        float pitch = -Mathf.Atan2(localDir.y, new Vector2(localDir.x, localDir.z).magnitude) * Mathf.Rad2Deg;
        pitch = Mathf.Clamp(pitch, minTilt, maxTilt);

        Quaternion current = tiltPole.localRotation;
        Quaternion target = Quaternion.Euler(pitch, 0f, 0f);

        tiltPole.localRotation = Quaternion.RotateTowards(current, target, tiltSpeed * Time.deltaTime);

        // sweep the dish

        if (dish)
        {
            dish.Rotate(60f * Time.deltaTime, 0f, 0f, Space.Self);
        }

    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }


} 
