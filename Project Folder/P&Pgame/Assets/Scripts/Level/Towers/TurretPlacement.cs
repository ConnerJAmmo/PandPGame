using System;
using Unity.VisualScripting;
using UnityEngine;

public class TurretPlacement : MonoBehaviour, ITurret
{
    [SerializeField] GameObject[] STTowers;
    [SerializeField] GameObject[] AOETowers;
    [SerializeField] GameObject forceField;
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] mined5Aud;
    [Range(0, 1)][SerializeField] float mined5AudVol;

    [Range(0, 3)] public int turretLevel;
    public bool hasShield = false;
    private int shieldHP;
    private int stoneCost;
    private int woodCost;
    private int towerGoldCost;
    private int upgradeCost;
    private int shieldCost;

    void Start()
    {
        stoneCost = gameManager.instance.towerStoneCost;
        woodCost = gameManager.instance.towerWoodCost;
        towerGoldCost = gameManager.instance.towerGoldCost;
        upgradeCost = gameManager.instance.towerUpgradeCost;
        shieldCost = gameManager.instance.towerShieldCost;
    }

    void ITurret.SpawnTower(char key)
    {
        //Debug.Log("SpawnTower called with: " + key);
        PlayerCont player = gameManager.instance.player.GetComponent<PlayerCont>();

        if (key == 'z')
        {
            if (turretLevel == 0 && player.woodCount >= woodCost && player.goldCount >= towerGoldCost)
            {
                Instantiate(STTowers[turretLevel++], 
                    new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z),
                    transform.rotation, transform);
                player.woodCount -= woodCost;
                gameManager.instance.removeGold(towerGoldCost);
                gameManager.instance.updateResourcesUI();
                //aud.PlayOneShot(mined5Aud[0], mined5AudVol);
            }
            else if (turretLevel != 0 && turretLevel != 3 && player.goldCount >= upgradeCost)
            {
                //Debug.Log("Upgrade Attempt");
                transform.GetComponent<ITurret>().UpgradeTower(key);
            }
        }
        else if (key == 'x')
        {
            if (turretLevel == 0 && player.stoneCount >= stoneCost && player.goldCount >= towerGoldCost)
            {
                Instantiate(AOETowers[turretLevel++], 
                    new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z),
                    transform.rotation, transform);
                player.stoneCount -= stoneCost;
                gameManager.instance.removeGold(towerGoldCost);
                gameManager.instance.updateResourcesUI();
                //aud.PlayOneShot(mined5Aud[0], mined5AudVol);
            }
            else if (turretLevel != 0 && turretLevel != 3 && player.goldCount >= upgradeCost)
            {
                //Debug.Log("Upgrade Attempt");
                transform.GetComponent<ITurret>().UpgradeTower(key);
            }
        }
    }

    void ITurret.UpgradeTower(char key)
    {
        //Debug.Log("Upgrade Registered");
        PlayerCont player = gameManager.instance.player.GetComponent<PlayerCont>();

        if (key == 'z' && transform.GetChild(1).gameObject.CompareTag("ST Turret"))
        {
            //Debug.Log("Upgrading Gun");
            if (hasShield)
            {
                shieldHP = transform.GetComponentInChildren<ForceField>().HP;
                //Debug.Log("Recording Shield Health: " + shieldHP);
            }
            Destroy(transform.GetChild(1).gameObject);
            GameObject newTower = Instantiate(STTowers[turretLevel++], 
                new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z), 
                transform.rotation, transform);
            if (hasShield)
            {
                Transform detectRadius = newTower.transform.GetChild(0);
                GameObject newShield = Instantiate(forceField, detectRadius.position, detectRadius.rotation, detectRadius);
                newShield.GetComponent<ForceField>().HP = shieldHP;
            }
            gameManager.instance.removeGold(upgradeCost);
            gameManager.instance.updateResourcesUI();
            //aud.PlayOneShot(mined5Aud[0], mined5AudVol);
        }
        else if (key == 'x' && transform.GetChild(1).gameObject.CompareTag("AOE Turret"))
        {
            //Debug.Log("Upgrading Rocket");
            if (hasShield)
            {
                shieldHP = transform.GetComponentInChildren<ForceField>().HP;
                //Debug.Log("Recording Shield Health: " + shieldHP);
            }
            Destroy(transform.GetChild(1).gameObject);
            GameObject newTower = Instantiate(AOETowers[turretLevel++],
                new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z),
                transform.rotation, transform);
            if (hasShield)
            {
                Transform detectRadius = newTower.transform.GetChild(0);
                GameObject newShield = Instantiate(forceField, detectRadius.position, detectRadius.rotation, detectRadius);
                newShield.GetComponent<ForceField>().HP = shieldHP;
            }
            gameManager.instance.removeGold(upgradeCost);
            gameManager.instance.updateResourcesUI();
            //aud.PlayOneShot(mined5Aud[0], mined5AudVol);
        }
    }

    void ITurret.ShieldGenerator()
    {
        //Debug.Log("Key Registered: c");
        PlayerCont player = gameManager.instance.player.GetComponent<PlayerCont>();
        Transform detectRadius = transform.GetChild(1).GetChild(0);

        if (turretLevel > 0 && player.goldCount >= shieldCost && !hasShield)
        {
            Instantiate(forceField,
                new Vector3(detectRadius.position.x, detectRadius.position.y, detectRadius.position.z),
                detectRadius.rotation, detectRadius);
            gameManager.instance.removeGold(shieldCost);
            gameManager.instance.updateResourcesUI();
            //aud.PlayOneShot(mined5Aud[0], mined5AudVol);
            hasShield = true;
        }
        // 1. Get the component once
        ForceField field = transform.GetComponentInChildren<ForceField>();

        if (field != null && field.HP < field.maxHP)
        {
            // 2. How much health does 1 gold buy?
            // Total HP divided by Total Cost = HP value of 1 gold unit
            float hpPerGold = (float)field.maxHP / shieldCost;

            // 3. Calculate missing HP and how much gold that equates to
            float missingHP = field.maxHP - field.HP;
            int goldNeeded = Mathf.CeilToInt(missingHP / hpPerGold);

            // 4. Cap by player's actual gold
            int goldToSpend = Mathf.Min(goldNeeded, player.goldCount);

            if (goldToSpend > 0)
            {
                // 5. Apply repair
                field.HP += (int)(goldToSpend * hpPerGold);
                field.HP = Mathf.Clamp(field.HP, 0, field.maxHP);

                gameManager.instance.removeGold(goldToSpend);
                gameManager.instance.updateResourcesUI();

                //Debug.Log($"Spent {goldToSpend} gold to repair {goldToSpend * hpPerGold} HP.");
            }
        }
    }
}
