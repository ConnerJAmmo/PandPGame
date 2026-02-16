using Unity.Burst.CompilerServices;
using UnityEngine;

public class TeleporterControlPanel : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] string padAId = "PadA";
    [SerializeField] string padBId = "PadB";
    [SerializeField] int requiredCrystals;

    [Header("Message")]
    [SerializeField] string msgNotEnough = "Collect and enter 6 crystals to activate teleporter";
    [SerializeField] string msgReady = "Press F to insert crystals";
    [SerializeField] string msgActivated = "Teleporter activated";

    [Header("Audio")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip activateSfx;
    [Range(0, 1)][SerializeField] float vol = 0.9f;

    [Header("Panel Visuals")]
    [SerializeField] PanelDoor door;
    [SerializeField] PanelScreenUI screen;

    bool playerInRange;
    Transform player;
    PlayerCrystalInventory inv;

    private void Update()
    {
        if (!playerInRange || !player) return;

        if (!inv) inv = player.GetComponent<PlayerCrystalInventory>();
        int count;

        if (inv)
            count = inv.Crystals;
        else
            count = 0;

        bool bothActive = PlayerTeleporter.AreBothActive(padAId, padBId);
        if (bothActive)
        {
            screen?.SetText("Teleporter online");
            return;
        }

        if (count < requiredCrystals)
        {
            screen?.SetText($"{msgNotEnough} ({count}/{requiredCrystals})");
            return;
        }

        screen?.SetText($"{msgReady} ({count}/{requiredCrystals})");

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (inv && inv.ConsumeCrystals(requiredCrystals))
            {
                //teleporterToActivate.Activate();
                PlayerTeleporter.ActivatePair(padAId, padBId);
                
                if (aud && activateSfx)
                    aud.PlayOneShot(activateSfx, vol);

                screen?.SetText(msgActivated);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;
        player = other.transform;
        inv = other.GetComponent<PlayerCrystalInventory>();

        int count;
        if (inv)
            count = inv.Crystals;
        else
            count = 0;
        door?.Open();
        screen?.SetText($"{msgNotEnough} ({count}/{requiredCrystals})");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        player = null;
        inv = null;

        door?.Close();
        screen?.Clear();
        gameManager.instance.ClearInteractionHint();
    }


}
