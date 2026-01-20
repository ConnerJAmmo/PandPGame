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
    [SerializeField] TMP_Text gameGoalText;
    [SerializeField] TMP_Text gameGoalTextOrig;
    [SerializeField] TMP_Text waveCountText;
    [SerializeField] TMP_Text waveCountTextOrig;
    [SerializeField] TMP_Text goldCountText;
    [SerializeField] TMP_Text woodCountText;
    [SerializeField] TMP_Text stoneCountText;
    [SerializeField] TMP_Text hintText;

    public bool isPause;
    public GameObject player;
    public PlayerCont playerScript;
    public GameObject baseTower;
    public int startingGold;

    public Image playerHPBar;
    public GameObject damageFlash;

    float timeScaleOrig;
    int goldCount;
    int gameGoalCount;
    int gameGoalCountOrig;


    void Awake()
    {
        instance = this;
        timeScaleOrig = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerCont>();

        UpdateGold(startingGold);
        goldCountText.text = goldCount.ToString("F0");

        SetWaveCountUI(5);

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
    }

    public void SetGameGoalOirgUI()
    {
        gameGoalCountOrig = gameGoalCount;
        gameGoalTextOrig.text = gameGoalCountOrig.ToString("F0");
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
    public void updateGameGoal(int amount)
    {
        gameGoalCount += amount;
        gameGoalText.text = gameGoalCount.ToString("F0");
        if (gameGoalCount <= 0)
        {
            newMenu(menuWin);
        }
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

    public void SetHint(string msg)
    {
        if (!hintText) return;

        hintText.text = msg;
        hintText.gameObject.SetActive(!string.IsNullOrEmpty(msg));
    }

    public void ClearHint()
    {
        SetHint("");
    }
}
