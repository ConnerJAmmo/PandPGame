using UnityEngine;
using UnityEngine.Windows;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [Header("Menus")]
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;

    public bool isPaused;
    public GameObject player;
    public GameObject baseTower;
    public playerControler playerScript;

    float timeScaleOrigin;

    int gameGoalCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
        timeScaleOrigin = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerControler>();

        baseTower = GameObject.FindWithTag("Base");
    }

    // Update is called once per frame
    void Update()
    {
        if(UnityEngine.Input.GetButtonDown("Cancel")) 
        {
            if (menuActive == null)
            {
                setMenu(menuPause);
            }
            else if(menuActive == menuPause)
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
        isPaused = true;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void stateUnpause()
    {
        isPaused = false;
        Time.timeScale = timeScaleOrigin;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
    }

    public void youLose()
    {
        setMenu(menuLose);
    }

    public void updateGameGoal(int amount)
    {
        gameGoalCount += amount;
        if (gameGoalCount <= 0)
        {
            setMenu(menuWin);
        }
    }

    public void loseGame()
    {
        setMenu(menuLose);
    }

    void setMenu(GameObject menu)
    {
        statePause();
        menuActive = menu;
        menuActive.SetActive(true);
    }

}
