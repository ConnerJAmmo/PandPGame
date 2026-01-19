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

    [SerializeField] TMP_Text gameGoalCountText;
    [SerializeField] TMP_Text woodCountText;
    [SerializeField] TMP_Text stoneCountText;

    public bool isPause;
    public GameObject player;
    public PlayerCont playerScript;
    public GameObject baseTower;

    public Image playerHPBar;
    public GameObject damageFlash;

    float timeScaleOrig;
    int gameGoalCount;

    void Awake()
    {
        instance = this;
        timeScaleOrig = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerCont>();

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
        gameGoalCountText.text = gameGoalCount.ToString("F0");
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
}
