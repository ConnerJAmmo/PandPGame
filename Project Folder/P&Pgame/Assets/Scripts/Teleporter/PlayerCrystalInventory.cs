using System;
using System.Collections;
using UnityEngine;

public class PlayerCrystalInventory : MonoBehaviour
{
    public int Crystals {  get; private set; }

    [Header("Hint Message")]
    [SerializeField] string crystalHint = "Crystal acquired. Deliver to Teleporter or SpaaceCradt to activate.";
    [SerializeField] float hintDuration = 4f;
    [SerializeField] float hintCoolDown = 6f; //prevents spam

    Coroutine hintRoutine;
    float nextHintTime = 0f;




    public void AddCrystals(int amount)
    {
        int before = Crystals;
        Crystals = Mathf.Max(0, Crystals + amount);

        // Only react when we actually gained crystals
        if (Crystals <= before)
            return;

        TryShowCrystalHint();
    }

    private void TryShowCrystalHint()
    {
        // If teleporter is already online, no need to nag
        bool teleporterOnline = PlayerTeleporter.AreBothActive("PadA", "PadB");

        bool shipOnline = FindAnyObjectByType<ShipTurretController>()?.IsActive == true;

        if (teleporterOnline) return;
        if (shipOnline) return;

        if (Time.time < nextHintTime) return;
        nextHintTime = Time.time + hintCoolDown;

        if (hintRoutine != null)
            StopCoroutine(hintRoutine);
        hintRoutine = StartCoroutine(HintRoutine());

    }

    IEnumerator HintRoutine()
    {
        gameManager.instance.SetInteractionHint(crystalHint);
        yield return new WaitForSeconds(hintDuration);
        gameManager.instance.ClearInteractionHint();
        hintRoutine = null;
    }

    public bool ConsumeCrystals(int amount)
    {
        if (Crystals < amount) 
            return false;
        Crystals -= amount;
        return true;
    }


}
