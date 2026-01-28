using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;
using System.Collections;


public class gameManager : MonoBehaviour, goldManage
{
    public static gameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuTowerUpgrade;
    [SerializeField] GameObject menuPlayerUpgrade;
    [SerializeField] GameObject needGunText;

    [SerializeField] TMP_Text playerHPText;
    [SerializeField] TMP_Text playerHPTextOrig;
    [SerializeField] TMP_Text enemyCountText;
    [SerializeField] TMP_Text enemyCountTextOrig;
    [SerializeField] TMP_Text waveCountText;
    [SerializeField] TMP_Text waveCountTextOrig;
    [SerializeField] TMP_Text goldCountText;
    [SerializeField] TMP_Text gunNameText;
    [SerializeField] TMP_Text damageCostText;
    [SerializeField] TMP_Text damageText;
    [SerializeField] TMP_Text damageLevelText;
    [SerializeField] TMP_Text fireRateCostText;
    [SerializeField] TMP_Text fireRateText;
    [SerializeField] TMP_Text fireRateLevelText;
    [SerializeField] TMP_Text RangeCostText;
    [SerializeField] TMP_Text RangeText;
    [SerializeField] TMP_Text RangeLevelText;
    [SerializeField] TMP_Text woodCountText;
    [SerializeField] TMP_Text stoneCountText;
    [SerializeField] TMP_Text ammoCountText;
    [SerializeField] TMP_Text hintText;

    [Header("Gun Upgrade Stats")]
    [SerializeField] public int damageUpgradeCost;
    [SerializeField] public int fireRateUpgradeCost;
    [SerializeField] public int rangeUpgradeCost;
    [SerializeField] int upgradeCostPreLevel;
    [SerializeField] public int damageLevel;
    [SerializeField] int damagePreLevel;
    [SerializeField] public int fireRateLevel;
    [SerializeField] float fireRatePreLevel;
    [SerializeField] public int rangeLevel;
    [SerializeField] int rangePreLevel;
    [SerializeField] public int maxLevel;

    [SerializeField] int upgradedis;
    [SerializeField] LayerMask towerLayer;

    public bool isPause;
    public bool waveActive;
    public GameObject player;
    public PlayerCont playerScript;
    public GameObject baseTower;
    public int startingGold;
    public int enemyCount;
    public int enemyCountOrig;
    public int goldCount;

    public Image playerHPBar;
    public GameObject damageFlash;

    float timeScaleOrig;

    string baseHint;
    string interactionHint;
    

    public GameObject waveSpawner;

    void Awake()
    {
        instance = this;
        timeScaleOrig = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerCont>();


        addGold(startingGold);
        goldCountText.text = goldCount.ToString("F0");

        waveSpawner = GameObject.FindWithTag("WaveSpawner");
        SetActiveWaveUI(1);

        baseTower = GameObject.FindWithTag("Base");

        updateResourcesUI();
    }

    
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);
            }
            else if (menuActive == menuPause) 
            {
                 stateUnpause();
            }
        }

        SetDamageUpgradeText();
        SetFireRateUpgradeText();
        SetRangeUpgradeText();
        openPlayerUpgradeMenu();
    }

    public void updateEnemyCountTotal(int amount)
    {
        enemyCountOrig += amount;
        enemyCountTextOrig.text = enemyCountOrig.ToString("F0");
    }

    public void updateEnemyCount(int amount)
    {
        enemyCount += amount;
        enemyCountText.text = enemyCount.ToString("F0");
    }

    public void SetHPOirgUI()
    {
        playerHPTextOrig.text = playerScript.HPOrig.ToString("F0");
    }

    public void SetHPUI()
    {
        playerHPText.text = playerScript.HP.ToString("F0");
    }

    public void SetWaveCountUI(int waveCounts)
    {
        waveCountTextOrig.text = waveCounts.ToString("F0");
    }

    public void SetActiveWaveUI(int wave)
    {
        waveCountText.text = wave.ToString("F0");
    }
    public void SetGunNameText()
    {
        gunNameText.text = playerScript.gunName;
    }


    public void SetDamageUpgradeText()
    {
        damageCostText.text = damageUpgradeCost.ToString("F0");
        damageText.text = gameManager.instance.playerScript.shootDamage.ToString("F0");
        damageLevelText.text = damageLevel.ToString("F0");
    }

    public void SetFireRateUpgradeText()
    {
        fireRateCostText.text = fireRateUpgradeCost.ToString("F0");
        fireRateText.text = gameManager.instance.playerScript.shootRate.ToString("F01");
        fireRateLevelText.text = fireRateLevel.ToString("F0");
    }
    public void SetRangeUpgradeText()
    {
        RangeCostText.text = rangeUpgradeCost.ToString("F0");
        RangeText.text = gameManager.instance.playerScript.shootDist.ToString("F0");
        RangeLevelText.text = rangeLevel.ToString("F0");
    }


    public int GetGold()
    {
        return goldCount;

    }

    public void newMenu(GameObject menu)
    {
        statePause();
        menuActive = menu;
        menuActive.SetActive(true);

    }

    public void statePause()
    {
        isPause = true;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void stateUnpause()
    {
        isPause = false;
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
    }

    public void openPlayerUpgradeMenu()
    {
        if (Input.GetButtonDown("Player Upgrade Menu") && playerScript.gunListPos > 0)
        {
            if (menuActive == null)
            {
                newMenu(menuPlayerUpgrade);
                SetDamageUpgradeText();
                SetFireRateUpgradeText();
                SetRangeUpgradeText();
            }
            else if (menuActive == menuPlayerUpgrade)
            {
                stateUnpause();
            }
        }
        //else
        //{
        //    StartCoroutine(flashNeedGun());
        //}

    }

    IEnumerator flashNeedGun()
    {
        needGunText.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        needGunText.SetActive(false);
    }

    public void youLose()
    {
        newMenu(menuLose);
    }

    public void youWin()
    {
        newMenu(menuWin);
    }

    public void updateResourcesUI()
    {
       if (playerScript == null)
        {
            return;
        }

        woodCountText.text = playerScript.woodCount.ToString("F0");
        stoneCountText.text = playerScript.stoneCount.ToString("F0");
    }

    public void RefreshHint()
    {
        if (!hintText) return;

        string final;
        if(!string.IsNullOrEmpty(interactionHint))
        { 
            final = interactionHint; 
        }else
        {
            final = baseHint;
        }



        hintText.text = final;
        hintText.gameObject.SetActive(!string.IsNullOrEmpty(final));
    }

    public void ClearInteractionHint()
    {
        interactionHint = "";
        RefreshHint();
    }

    public void SetInteractionHint(string msg)
    {
        interactionHint = msg; 
        RefreshHint() ;
    }

    public void SetBaseHint(string msg)
    {
        baseHint = msg; 
        RefreshHint() ;
    }

    public void addGold(int gold)
    {
        goldCount += gold;
        goldCountText.text = goldCount.ToString("F0");
    }

    public void removeGold(int gold)
    {
        goldCount -= gold;
        goldCountText.text = goldCount.ToString("F0");
    }

    public int upgradeDamage(int baseDamage)
    {
        int damage = baseDamage + (damageLevel + damagePreLevel);
        return damage;
    }

    public float upgradeRate(float baseRate)
    {
        float rate = Mathf.Max(0.05f, baseRate - fireRatePreLevel);
        return rate;
    }

    public int upgradeRange(int baseRange)
    {
        int range = baseRange + (rangeLevel + rangePreLevel);
        return range;
    }

    public void UpdateAmmoUI(int currentAmmo, int maxAmmo)
    {
        if (ammoCountText != null)
        {
            ammoCountText.text = currentAmmo.ToString() + " / " + maxAmmo.ToString();
        }
    }

    public void upgradePlayerShootDamage()
    {
        gameManager.instance.playerScript.gunList[gameManager.instance.playerScript.gunListPos]
                .shootDamage += upgradeDamage(gameManager.instance.playerScript.shootDamage);
        damageLevel++;
        removeGold(damageUpgradeCost);
        damageUpgradeCost += upgradeCostPreLevel;
        SetDamageUpgradeText();
        gameManager.instance.playerScript.ChangeGun();
    }
    public void upgradePlayerShootRate()
    {
        gameManager.instance.playerScript.gunList[gameManager.instance.playerScript.gunListPos]
                .shootRate += upgradeRate(gameManager.instance.playerScript.shootRate);
        fireRateLevel++;
        removeGold(fireRateUpgradeCost);
        fireRateUpgradeCost += upgradeCostPreLevel;
        SetFireRateUpgradeText();
        gameManager.instance.playerScript.ChangeGun();
    }
    public void upgradePlayerShootRange()
    {
        gameManager.instance.playerScript.gunList[gameManager.instance.playerScript.gunListPos]
                .shootDist += upgradeRange(gameManager.instance.playerScript.shootDist);
        rangeLevel++;
        removeGold(rangeUpgradeCost);
        rangeUpgradeCost += upgradeCostPreLevel;
        SetRangeUpgradeText();
        gameManager.instance.playerScript.ChangeGun();
    }
}
