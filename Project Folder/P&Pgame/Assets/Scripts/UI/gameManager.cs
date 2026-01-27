using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class gameManager : MonoBehaviour, goldManage
{
    public static gameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuTowerUpgrade;
    [SerializeField] GameObject menuPlayerUpgrade;

    [SerializeField] TMP_Text playerHPText;
    [SerializeField] TMP_Text playerHPTextOrig;
    [SerializeField] TMP_Text enemyCountText;
    [SerializeField] TMP_Text enemyCountTextOrig;
    [SerializeField] TMP_Text waveCountText;
    [SerializeField] TMP_Text waveCountTextOrig;
    [SerializeField] TMP_Text goldCountText;
    [SerializeField] TMP_Text woodCountText;
    [SerializeField] TMP_Text stoneCountText;
    [SerializeField] TMP_Text ammoCountText;
    [SerializeField] TMP_Text hintText;


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

    public void openUpgradeMenu()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void closeUpgradeMenu()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
    }

   

    public void openPlayerUpgradeMenu()
    {
        if (Input.GetButtonDown("Player Upgrade Menu"))
        {
            if (menuActive == null)
            {
                openUpgradeMenu();
                menuActive = menuPlayerUpgrade;
                menuActive.SetActive(true);
            }
            else if (menuActive == menuPlayerUpgrade)
            {
                closeUpgradeMenu();
            }
        }
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

    public void UpdateAmmoUI(int currentAmmo, int maxAmmo)
    {
        if (ammoCountText != null)
        {
            ammoCountText.text = currentAmmo.ToString() + " / " + maxAmmo.ToString();
        }
    }
}
