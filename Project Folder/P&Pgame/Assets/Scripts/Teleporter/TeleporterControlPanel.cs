using System;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public class TeleporterControlPanel : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] string padAId = "PadA";
    [SerializeField] string padBId = "PadB";
    [SerializeField] int requiredCrystals = 7;

    [Header("Message")]
    [SerializeField] string msgNotEnough = "Collect crystals to activate teleporter";
    [SerializeField] string msgReady = "Press F to insert a crystals";
    [SerializeField] string msgActivated = "Teleporter activated";

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

    [Header("PowerLines")]
    [SerializeField] ElectricLine[] powerLines; // Size = requiredCrystals
    [SerializeField] AudioClip LineOnSfx;
    [SerializeField] float lineOnVol;

    int inserted = 0;

    bool playerInRange;
    Transform player;
    PlayerCrystalInventory inv;

    private void Awake()
    {
        // All lines start off
        if (powerLines != null)
        {
            for (int i = 0; i < powerLines.Length; i++)
            {
                if (powerLines[i])
                    powerLines[i].TurnOff();
            }
        }
    }

    private void Update()
    {
        if (!playerInRange || !player) return;

        if (!inv) inv = player.GetComponent<PlayerCrystalInventory>();
        int count;

        if (inv)
            count = inv.Crystals;
        else
            count = 0;

        // If already active, Lock UI
        bool bothActive = PlayerTeleporter.AreBothActive(padAId, padBId);
        if (bothActive)
        {
            screen?.SetText("Teleporter online");
            return;
        }
        
        // If already enough crystals, activate (failsafe)
        if (inserted >= requiredCrystals)
        {
            ActivateTeleporter();
            return;
        }

        //Show message
        if (count <= 0)
        {
            screen?.SetText($"{msgNotEnough} ({count}/{requiredCrystals})");
            return;
        }

        screen?.SetText($"{msgReady} ({inserted} / {requiredCrystals})  You have: {count}");

        //screen?.SetText($"{msgReady} ({count}/{requiredCrystals})");

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!inv) return;

            if (inv.ConsumeCrystals(1))
            {
                inserted++;

                int lineIndex = inserted - 1;
                if (powerLines != null && lineIndex >= 0 && lineIndex < powerLines.Length && powerLines[lineIndex])
                    powerLines[lineIndex].TurnOn();

                if (aud && LineOnSfx)
                    aud.PlayOneShot(LineOnSfx, lineOnVol);

                // If Done Activate portal
                if (inserted >= requiredCrystals)
                {
                    ActivateTeleporter();
                }
                else
                {
                    // Update text immediately after insert
                    int newCount = inv.Crystals;
                    screen?.SetText($"{msgReady} ({inserted}/{requiredCrystals})  You have {newCount}");

                }
            }
            else
            {
                // consume failed
                screen?.SetText($"{msgNotEnough} ({inserted}/{requiredCrystals})");
            }
        }
    }
                    
            

    private void ActivateTeleporter()
    {
        // Turn on any remaining lines just in case array is shorter
        if (powerLines != null)
        {
            for (int i = 0; i < powerLines.Length; i++)
                if (powerLines[i])
                    powerLines[i].TurnOn();
        }

        PlayerTeleporter.ActivatePair(padAId, padBId);

        if (aud && activateSfx)
            aud.PlayOneShot(activateSfx, vol);

        screen?.SetText(msgActivated);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;
        player = other.transform;
        inv = other.GetComponent<PlayerCrystalInventory>();

        door?.Open();
        if (aud && audDoorOpen)
            aud.PlayOneShot(audDoorOpen, audDoorOpenVol);

        int count;
        if (inv)
            count = inv.Crystals;
        else
            count = 0;
        screen?.SetText($"{msgNotEnough} ({inserted}/{requiredCrystals})  You have {count}");
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
