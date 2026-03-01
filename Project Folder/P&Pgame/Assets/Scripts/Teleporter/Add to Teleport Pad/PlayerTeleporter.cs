using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.VisualScripting;
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

    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip audTeleport;

    [SerializeField] PortalVFXController thisPadVFX;
    [SerializeField] PortalVFXController destinationPadVFX;

    [Header("Activation")]
    [SerializeField] bool startsActive = false;
    PortalVisualController visuals;
    public bool IsActive { get; private set; } 

    [Range(0, 1)] [SerializeField] float teleportVol;


    Transform cacheDestinationSpawn;


    private void Awake()
    {
        if (!visuals) visuals = GetComponent<PortalVisualController>();
        if (!visuals) visuals = GetComponentInChildren<PortalVisualController>(true);
        if (!visuals) visuals = GetComponentInParent<PortalVisualController>();

        IsActive = startsActive;

        if (visuals)
        {
            if (IsActive)
                visuals.TurnOn();
            else
                visuals.TurnOff();
        }

        cacheDestination();

    }

    public void Activate()
    {
        IsActive = true;
        visuals?.TurnOn();
        // turnOn portal visuals
        GetComponentInParent<PortalVFXController>()?.PlayOn();
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
        if (!IsActive) return;
        if (!other.CompareTag("Player")) return;

        if (!cacheDestinationSpawn)
        {
            cacheDestination();
            if (!cacheDestinationSpawn) return;
        }

        aud.PlayOneShot(audTeleport, teleportVol);

        if (thisPadVFX)
        {
            Debug.Log("Enter VFX Called", this);
            thisPadVFX.PlayEnter();
        }else
        {
            Debug.LogWarning("thisPadVFX is not assigned", this);
        }

        TeleportPostFX postFX = FindAnyObjectByType<TeleportPostFX>();

        if (postFX)
            postFX.Flash();

        SimpleCameraShake camShake = FindAnyObjectByType<SimpleCameraShake>();
        if (camShake)
            camShake.Shake();

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

        if (destinationPadVFX)
            destinationPadVFX.PlayExit();
            
    }

    public static void ActivatePair(string padAId, string padBId)
    {
        var pads = Object.FindObjectsByType<PlayerTeleporter>(FindObjectsSortMode.None);

        foreach (var pad in pads)
        {
            if (pad == null) continue;

            if (pad.padId == padAId || pad.padId == padBId)
            {
                pad.Activate();
            }
        }
    }

    public static bool AreBothActive(string padAId, string padBId)
    {
        bool a = false, b = false;

        var pads = Object.FindObjectsByType<PlayerTeleporter>(FindObjectsSortMode.None);
        foreach (var pad in pads)
        {
            if (!pad) 
            {
                continue;
            }
            if (pad.padId == padAId) a = pad.IsActive;
            if (pad.padId == padBId) b = pad.IsActive;
        }

        return a && b;
    }
}
