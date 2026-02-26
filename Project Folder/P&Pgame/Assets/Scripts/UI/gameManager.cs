using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;
using System.Collections;
using System;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Linq;



public class gameManager : MonoBehaviour, goldManage
{
    public static gameManager instance;
    [SerializeField] SaveBridge saveBridge;
    
#region Menus
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuPauseFristButton;
    [SerializeField] GameObject menuPlayerLose;
    [SerializeField] GameObject menuPlayerLoseFristButton;
    [SerializeField] GameObject menuTowerLose;
    [SerializeField] GameObject menuTowerLoseFristButton;

    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuWinFristButton;

    [SerializeField] GameObject menuTowerUpgrade;
    [SerializeField] GameObject menuTowerUpgradeFristButton;

    [SerializeField] GameObject menuPlayerUpgrade;
    [SerializeField] GameObject menuPlayerUpgradeFristButton;
    
    [SerializeField] GameObject needGunText;
    public bool isPause;
#endregion
    
#region Text Fields
    [Header("Text  Fields")]
    [SerializeField] TMP_Text playerHPText;
    [SerializeField] TMP_Text playerHPTextOrig;
    [Space]
    [SerializeField] TMP_Text enemyCountText;
    [SerializeField] TMP_Text enemyCountTextOrig;
    [Space]
    [SerializeField] TMP_Text waveCountText;
    [SerializeField] TMP_Text waveCountTextOrig;
    [Space]
    [SerializeField] TMP_Text goldCountText;
    [SerializeField] TMP_Text gunNameText;
    [Space]
    [SerializeField] TMP_Text damageCostText;
    [SerializeField] TMP_Text damageText;
    [SerializeField] TMP_Text damageLevelText;
    [Space]
    [SerializeField] TMP_Text maxAmmoCostText;
    [SerializeField] TMP_Text maxAmmoText;
    [SerializeField] TMP_Text maxAmmoLevelText;
    [Space]
    [SerializeField] TMP_Text RangeCostText;
    [SerializeField] TMP_Text RangeText;
    [SerializeField] TMP_Text RangeLevelText;
    [Space]
    [SerializeField] TMP_Text woodCountText;
    [SerializeField] TMP_Text stoneCountText;
    [SerializeField] TMP_Text metalCountText;
    [SerializeField] TMP_Text ammoCountText;
    [SerializeField] TMP_Text mothershipAmmoCountText;
    [SerializeField] TMP_Text mothershipCrystalCountText;
    [Space]
    [SerializeField] TMP_Text TowerHPMax;
    [SerializeField] TMP_Text TowerHP;
    [SerializeField] TMP_Text hintText;
#endregion

    [Header("Notification")]
    [SerializeField] GameObject notificationPanel;
    [SerializeField] TMP_Text notificationText;
    [SerializeField] float notificationDuration = 2f;

    [Header("CutScene")]
    [SerializeField] EndCutSceneManager endCutScene; //drag our cutscene director here
    [SerializeField] string mothershipSceneName = "Mothership";

#region Gun Upgrade Stats
    [Header("Gun Upgrade Stats")]
    [SerializeField] public int damageUpgradeCost;
    [SerializeField] public int maxAmmoUpgradeCost;
    [SerializeField] public int rangeUpgradeCost;
    [SerializeField] int upgradeCostPreLevel;
    [SerializeField] public int damageLevel;
    [SerializeField] int damagePreLevel;
    [SerializeField] public int maxAmmoLevel;
    [SerializeField] int maxAmmoPreLevel;
    [SerializeField] public int rangeLevel;
    [SerializeField] int rangePreLevel;
    [SerializeField] public int maxLevel;

    public int initialDamageUpgradeCost = 10;
    public int initalMaxAmmoUpgradeCost = 10;
    public int initialRangeUpgradeCost = 10;
    int mothershipCrystalMaxCount;
    public int mothershipCrystalCurrentCount;
    GameObject[] crystalInMothership;
    #endregion

#region Tower Costs
    [Header("Tower Costs")]
    [Range(0, 100)][SerializeField] public int towerStoneCost;
    [Range(0, 100)][SerializeField] public int towerWoodCost;
    [Range(0, 100)][SerializeField] public int towerGoldCost;
    [Range(0, 100)][SerializeField] public int towerUpgradeCost;
    [Range(0, 100)][SerializeField] public int towerShieldCost;
    #endregion

    public bool waveActive;
    public GameObject player;
    public PlayerCont playerScript;
    public GameObject baseTower;
    public baseDmg baseTowerScript;
    public int startingGold;
    public int enemyCount;
    public int enemyCountOrig;
    public int goldCount;

    public Image playerHPBar;
    public Image towerHPBar;
    public GameObject damageFlash;

    float timeScaleOrig;

    string baseHint;
    string interactionHint;

    Scene currentScene;

    public GameObject waveSpawner;

#region Audio
    [Header("-------------Audio--------------")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip menuInteractionAud;
    [SerializeField] float menuVol;
    [SerializeField] AudioClip deathAud;
    [SerializeField] float deathVol;
    [SerializeField] AudioClip mainMusic;
    [SerializeField] float mainMusicVol;
#endregion
    
    void Awake()
    {
        instance = this;
        timeScaleOrig = Time.timeScale;

        

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerCont>();
        baseTower = GameObject.FindWithTag("Base");
        baseTowerScript = baseTower.GetComponent<baseDmg>();

        damageLevel = 0;
        maxAmmoLevel = 0;
        rangeLevel = 0;

        damageUpgradeCost = initialDamageUpgradeCost;
        maxAmmoUpgradeCost = initalMaxAmmoUpgradeCost;
        rangeUpgradeCost = initialRangeUpgradeCost;
        addGold(startingGold);
        goldCountText.text = goldCount.ToString("F0");

        currentScene = SceneManager.GetActiveScene();

        waveSpawner = GameObject.FindWithTag("WaveSpawner");
        SetActiveWaveUI(1);

        crystalInMothership = GameObject.FindGameObjectsWithTag("Crystal");
        mothershipCrystalMaxCount = crystalInMothership.Count();

        updateResourcesUI();
        updateEnemyCount(0);
        updateEnemyCountTotal(0);

        // Bridge to connect all the save functions and scripts together
        if (!saveBridge)
            saveBridge = FindFirstObjectByType<SaveBridge>();

        // The (?) is a chaining operator so if savebridge is null are game won't crash
        saveBridge?.ApplyLoadedData();
    }

    
    void Update()
    {
        if (menuActive == null)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(menuPauseFristButton);
                aud.PlayOneShot(menuInteractionAud, menuVol);
            }
            else if (menuActive == menuPause) 
            {
                 stateUnpause();
            }
            else if (menuActive == menuPlayerUpgrade)
            {
                menuActive.SetActive(false);
                menuActive = menuPause;
                menuActive.SetActive(true);
            }
        }

        SetDamageUpgradeText();
        SetMaxAmmoUpgradeText();
        SetRangeUpgradeText();
        openPlayerUpgradeMenu();
        SetMothershipCrystalCountText();
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

    public void SetPlayerHPOirgUI()
    {
        playerHPTextOrig.text = playerScript.HPOrig.ToString("F0");
    }

    public void SetPlayerHPUI()
    {
        playerHPText.text = playerScript.HP.ToString("F0");
    }
    public void SetTowerHPUI()
    {
        TowerHP.text = baseTowerScript.hp.ToString("F0");
    }

    public void SetTowerHPOirgUI()
    {
        TowerHPMax.text = baseTowerScript.maxHP.ToString("F0");
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
    
    public void SetMothershipCrystalCountText()
    {
        mothershipCrystalCountText.text = mothershipCrystalCurrentCount.ToString() + " - " + mothershipCrystalMaxCount.ToString();
    }

    public void SetDamageUpgradeText()
    {
        damageCostText.text = damageUpgradeCost.ToString("F0");
        damageText.text = playerScript.shootDamage.ToString("F0");
        damageLevelText.text = damageLevel.ToString("F0");
    }

    public void SetMaxAmmoUpgradeText()
    {
        maxAmmoCostText.text = maxAmmoUpgradeCost.ToString("F0");
        maxAmmoText.text = playerScript.maxAmmo.ToString("F0");
        maxAmmoLevelText.text = maxAmmoLevel.ToString("F0");
    }
    public void SetRangeUpgradeText()
    {
        RangeCostText.text = rangeUpgradeCost.ToString("F0");
        RangeText.text = playerScript.shootDist.ToString("F0");
        RangeLevelText.text = rangeLevel.ToString("F0");
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
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void stateUnpause()
    {
        isPause = false;
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
        //aud.PlayOneShot(menuInteractionAud, menuVol);
    }
    public void openPause()
    {
        menuActive.SetActive(false);
        menuActive = menuPause;
        menuActive.SetActive(true);
    }

    public void openPlayerUpgradeMenu()
    {
        if (Input.GetButtonDown("Player Upgrade Menu"))
        {
            if (menuActive == null && playerScript.gunList.Count > 0)
            {
                newMenu(menuPlayerUpgrade);
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(menuPlayerUpgradeFristButton);
                SetDamageUpgradeText();
                SetMaxAmmoUpgradeText();
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

    public void youLosePlayer()
    {
        newMenu(menuPlayerLose);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(menuPlayerLoseFristButton);
        aud.PlayOneShot(deathAud, deathVol);
    }
    public void youLoseTower()
    {
        newMenu(menuTowerLose);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(menuTowerLoseFristButton);
        aud.PlayOneShot(deathAud, deathVol);
    }

    public void youWin()
    {
        // if we are in the Mothership (our last level) play cutscene if we win
        if (SceneManager.GetActiveScene().name == mothershipSceneName && endCutScene != null)
        {
            // Make sure game isn't paused
            if (menuActive != null) stateUnpause();

            endCutScene.StartEndCutscene();
            return;
        }

        //Default behavior for other levels
        newMenu(menuWin);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(menuWinFristButton);
    }

    public void updateResourcesUI()
    {
       if (playerScript == null)
        {
            return;
        }

        woodCountText.text = playerScript.woodCount.ToString("F0");
        stoneCountText.text = playerScript.stoneCount.ToString("F0");
        //metalCountText.text = playerScript.metalCount.ToString("F0");
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
        int damage = baseDamage += damagePreLevel;
        return damage;
    }

    public int upgradeMaxAmmo(int baseMaxAmmo)
    {
        int maxAmmo = baseMaxAmmo += maxAmmoPreLevel;
        return maxAmmo;
    }

    public int upgradeRange(int baseRange)
    {
        int range = (baseRange += rangePreLevel);
        return range;
    }

    public void UpdateAmmoUI(int currentAmmo, int maxAmmo)
    {
        if (ammoCountText != null)
        {
            // To update the mothership UI for ammo and crystal counts
            if (currentScene.name == "Mothership")
            {
                mothershipAmmoCountText.text = currentAmmo.ToString() + " - " + maxAmmo.ToString(); 
            }
            // To update all other UI ammo count
            else
            {
                ammoCountText.text = currentAmmo.ToString() + " - " + maxAmmo.ToString();
            }
        }
    }

    public void upgradePlayerShootDamage()
    {
        playerScript.gunList[playerScript.gunListPos]
                .shootDamage = upgradeDamage(playerScript.gunList[playerScript.gunListPos].shootDamage);
        playerScript.gunList[playerScript.gunListPos].damageLevel++;
        removeGold(damageUpgradeCost);
         playerScript.gunList[playerScript.gunListPos].damageUpgradeCost += upgradeCostPreLevel;
        SetDamageUpgradeText();
        playerScript.ChangeGun();
    }
    public void upgradePlayerMaxAmmo()
    {
        playerScript.gunList[playerScript.gunListPos]
            .ammoMax = upgradeMaxAmmo(playerScript.gunList[playerScript.gunListPos].ammoMax);
        playerScript.gunList[playerScript.gunListPos].maxAmmoLevel++;
        removeGold(maxAmmoUpgradeCost);
        playerScript.gunList[playerScript.gunListPos].maxAmmoUpgradeCost += upgradeCostPreLevel;
        SetMaxAmmoUpgradeText();
        UpdateAmmoUI(playerScript.gunList[playerScript.gunListPos].ammoCur, playerScript.gunList[playerScript.gunListPos].ammoMax);
        playerScript.ChangeGun();
    }
    public void upgradePlayerShootRange()
    {
        playerScript.gunList[playerScript.gunListPos]
                .shootDist = upgradeRange(playerScript.shootDist);
        playerScript.gunList[playerScript.gunListPos].DistLevel++;
        removeGold(rangeUpgradeCost);
        playerScript.gunList[playerScript.gunListPos].rangeUpgradeCost += upgradeCostPreLevel;
        SetRangeUpgradeText();

        playerScript.ChangeGun();
    }

    public void ShowNotification(string message)
    {
        if (notificationPanel != null && notificationText != null)
        {
            StartCoroutine(DisplayNotification(message));
        }
    }

    private IEnumerator DisplayNotification(string message)
    {
        notificationText.text = message;
        notificationPanel.SetActive(true);
        yield return new WaitForSeconds(notificationDuration);
        notificationPanel.SetActive(false);
    }


    public int GetGold()
    {
        return goldCount;
    }


    public void SetGold(int value)
    {
        goldCount = value; 
        goldCountText.text = goldCount.ToString("F0");
    }

    public void LevelComplete()
    {
        
        int current = SceneManager.GetActiveScene().buildIndex;

        int next;

        if (current == 2) //Outpost
        {
            next = 3;     //Gorge
        }
        else if (current == 3)//Gorge
            next = 4;     //Mothership
        else
            next = 1;     // Back to mainmenu
        
        
        CompleteLevelAndLoadNext(next);

    }
    
    // ----------------These Method are used for the advancing------------------------//
    
    public void CompleteLevelAndLoadNext(int nextSceneIndex)
    {

        if (!saveBridge)
            saveBridge = FindFirstObjectByType<SaveBridge>();
        saveBridge?.CollectAndSave();

    
        // record progress as "next scene" (So Continue resumes next level)
        GameSession.instance.Data.currentLevelIndex = nextSceneIndex;
        GameSession.instance.SaveGame();

        

        SceneManager.LoadScene(nextSceneIndex);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    } 
    
    // ------------------------------End---------------------------------------//


}




