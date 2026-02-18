using Unity.Burst.CompilerServices;
using UnityEngine;

public class ShipTurretControlPanel : MonoBehaviour
{
    [Header("Setup")]

    [SerializeField] ShipTurretController turret;
    [SerializeField] int requiredCrystals;

    [Header("Message")]
    [SerializeField] string msgNotEnough = "Collect and enter 6 crystals to activate teleporter";
    [SerializeField] string msgReady = "Press F to insert crystals";
    [SerializeField] string msgActivated = "Ship Turret activated";

    [Header("Audio")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip activateSfx;
    [Range(0, 1)][SerializeField] float audActivateVol;
    [SerializeField] AudioClip audDoorOpen;
    [Range(0, 1)][SerializeField] float audDoorOpenVol;
    [Range(0, 1)][SerializeField] float vol = 0.9f;

    [Header("Panel Visuals")]
    [SerializeField] PanelDoor door;
    [SerializeField] PanelScreenUI screen;

    bool playerInRange;
    Transform player;
    PlayerCrystalInventory inv;

    private void Update()
    {
        if (!playerInRange || !turret) return;

        if (turret.IsActive)
            { screen?.SetText(msgActivated); return; }


        if (!inv) inv = player.GetComponent<PlayerCrystalInventory>();
        int count;

        if (inv)
            count = inv.Crystals;
        else
            count = 0;


        



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
                turret.Activate();

                screen?.SetText("Turret Online");
                
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

        if (aud && audDoorOpen)
            aud.PlayOneShot(audDoorOpen, audDoorOpenVol);
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
