using UnityEngine;

public class PlayerCrystalInventory : MonoBehaviour
{
  
    public int Crystals {  get; private set; }

    public void AddCrystals(int amount)
    {
        Crystals = Mathf.Max(0, Crystals + amount);
    }

    public bool ConsumeCrystals(int amount)
    {
        if (Crystals < amount) 
            return false;
        Crystals -= amount;
        return true;
    }


}
