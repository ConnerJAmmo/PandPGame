using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject menuWin;

    [SerializeField] TMP_Text playerHPText;
    [SerializeField] TMP_Text playerHPTextOrig;
    [SerializeField] TMP_Text enemyCountText;
    [SerializeField] TMP_Text enemyCountTextOrig;
    [SerializeField] TMP_Text waveCountText;
    [SerializeField] TMP_Text waveCountTextOrig;
    [SerializeField] TMP_Text goldCountText;
    [SerializeField] TMP_Text woodCountText;
    [SerializeField] TMP_Text stoneCountText;

    public bool isPause;
    public bool waveActive;
    public GameObject player;
    public PlayerCont playerScript;
    public GameObject baseTower;
    public int startingGold;
    public int enemyCount;
    public int enemyCountOrig;

    public Image playerHPBar;
    public GameObject damageFlash;

    float timeScaleOrig;
    int goldCount;

    public GameObject waveSpawner;

    void Awake()
    {
        instance = this;
        timeScaleOrig = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerCont>();

        UpdateGold(startingGold);
        goldCountText.text = goldCount.ToString("F0");

        waveSpawner = GameObject.FindWithTag("WaveSpawner");
        SetWaveCountUI(0);
        SetActiveWaveUI(0);

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


    public void UpdateGold(int amount)
    {
        goldCount += amount;
        goldCountText.text = goldCount.ToString("F0");
    }

    public void newMenu(GameObject menu)
    {
        statePause();
        menuActive = menu;
        menuActive.SetActive(true );

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

    public void youLose()
    {
        newMenu(menuLose);
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
}
