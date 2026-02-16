using UnityEngine;
using System.Collections.Generic;
using System;


[Serializable]
public class SaveData // Not derived from MonoBehaviour
{
    public int version = 1;

    // Progress of levels
    public int lastUnlockedLevel = 3;
    public int currentLevelIndex = 3;

    // Player stats/resources
    public int playerHp = 100;
    public int gold = 0;
    public int wood = 0;
    public int stone = 0;

    // These are future-proof buckets (add later without breaking anything)

    // If we want to save unlocked upgrades later
    public List<string> unlockedUpgrades = new List<string>();

    // if we want to save stats later
    public Dictionary<string, int> stats = new Dictionary<string, int>();

    // if we want to save xp (experience points) later
    public int xp;

    // if we want to save inventory later

    public string[] inventory;
    


}
