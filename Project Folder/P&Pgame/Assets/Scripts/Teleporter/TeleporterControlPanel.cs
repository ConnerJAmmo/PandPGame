using Unity.Burst.CompilerServices;
using UnityEngine;

public class TeleporterControlPanel : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] PlayerTeleporter teleporterToActivate;
    [SerializeField] int requiredCrystals;

    [Header("Message")]
    [SerializeField] string msgNotEnough = "Collect and enter 6 crystals to activate teleporter";
    [SerializeField] string msgReady = "Press E to insert crystals";
    [SerializeField] string msgActivated = "Teleporter activated";

    [Header("Audio")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip activateSfx;
    [Range(0, 1)][SerializeField] float vol = 0.9f;

    bool playerInRange;
    Transform player;
    PlayerCrystalInventory inv;

    private void Update()
    {
        if (!playerInRange || !player || !teleporterToActivate) return;

        if (teleporterToActivate.IsActive)
        {
            gameManager.instance.SetInteractionHint("Teleporter online");
            return;
        }

        if (!inv) inv = player.GetComponent<PlayerCrystalInventory>();
        int count;

        if (inv)
            count = inv.Crystals;
        else
            count = 0;

        if (count < requiredCrystals)
        {
            gameManager.instance.SetInteractionHint($"{msgNotEnough} ({count}/{requiredCrystals})");
            return;
        }

        gameManager.instance.SetInteractionHint($"{msgReady} ({count}/{requiredCrystals}");

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (inv && inv.ConsumeCrystals(requiredCrystals))
            {
                teleporterToActivate.Activate();

                if (aud && activateSfx)
                    aud.PlayOneShot(activateSfx, vol);

                gameManager.instance.SetInteractionHint(msgActivated);
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
        gameManager.instance.SetInteractionHint($"{msgNotEnough} ({count}/{requiredCrystals})");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        player = null;
        inv = null;

        gameManager.instance.ClearInteractionHint();
    }


}
