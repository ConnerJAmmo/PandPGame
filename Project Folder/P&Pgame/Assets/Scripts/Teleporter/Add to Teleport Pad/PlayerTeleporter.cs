using UnityEngine;

public class PlayerTeleporter : MonoBehaviour
{
    [Header("Second Pad Destination")]
    [SerializeField] Transform destination; // Pad 2 destination

    [Header("Spawnpoint offset")]
    [SerializeField] Transform destinationPoint;

    [Header("Cooldown (No Instant Re - Teleport")]
    [SerializeField] float coolDown;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!destination) return;

        // Cooldown check for Player
        var allowed = other.GetComponent<teleportAllowed>();
        if (!allowed) allowed = other.gameObject.AddComponent<teleportAllowed>(); // Just add the teleport allowed script to player if its not there
        if (!allowed.CanTeleport()) return;

        Vector3 targetPos;

        if (destinationPoint)
        {
            targetPos = destinationPoint.position;
        }
        else
        {
            targetPos = destination.position;
        }

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
