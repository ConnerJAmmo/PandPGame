using UnityEngine;
using System.Collections.Generic;
using System;


[Serializable]
public class SaveData // Not derived from MonoBehaviour
{
    public int version = 1;

    // Progress of levels
    public int currentLevelIndex = 0;

    // Player stats/resources
    public int playerHp = 100;
    public int gold = 0;
    public int wood = 0;
    public int stone = 0;

    // These are future-proof buckets (add later without breaking anything)
    public List<string> unlockedUpgrades = new List<string>();
    public Dictionary<string, int> stats = new Dictionary<string, int>();
    


}
