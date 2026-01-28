using UnityEngine;

public class PlayerTeleporter : MonoBehaviour
{
    [Header("Paring")]
    [SerializeField] string padId = "PadA";
    [SerializeField] string destinationPadId = "PadB"; // Pad 2 destination

    [Header("Spawnpoint On THIS pad (optional)")]
    [SerializeField] Transform destinationPoint;

    [Header("Cooldown (No Instant Re - Teleport")]
    [SerializeField] float coolDown;

    Transform cacheDestinationSpawn;


    private void Awake()
    {
        cacheDestination();
    }

    void cacheDestination()
    {
        cacheDestinationSpawn = null;

        var pads = Object.FindObjectsByType<PlayerTeleporter>(FindObjectsSortMode.None);

        foreach (var pad in pads)
        {
            if (pad != null && pad.padId == destinationPadId)
            {
                if (pad.destinationPoint)
                {
                    cacheDestinationSpawn = pad.destinationPoint;
                }
                else
                {
                    cacheDestinationSpawn = pad.transform;
                }
                break;
            }
        }
        // just making sure there is a destination
        if (!cacheDestinationSpawn)
        {
            Debug.LogWarning($"Teleporter [{padId}] could not find destination [{destinationPadId}]");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!cacheDestinationSpawn)
        {
            cacheDestination();
            if (cacheDestinationSpawn) return;
        }

        // Cooldown check for Player
        var allowed = other.GetComponent<teleportAllowed>();
        if (!allowed) allowed = other.gameObject.AddComponent<teleportAllowed>(); // Just add the teleport allowed script to player if its not there
        if (!allowed.CanTeleport()) return;

        Vector3 targetPos = cacheDestinationSpawn.position;

        // CharacterController safe
        var cc = other.GetComponent<CharacterController>();
        if (cc)
        {
            cc.enabled = false;
            other.transform.position = targetPos;
            cc.enabled = true;
        }
        else
        {
            other.transform.position = targetPos;
        }

        allowed.SetCooldown(coolDown);

    }
}
