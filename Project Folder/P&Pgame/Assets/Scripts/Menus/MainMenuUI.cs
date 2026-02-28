using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;


public class MainMenuUI : MonoBehaviour
{
    [Header("Gameplay Scene Indexes")]

    [SerializeField] string outpostScene;
    [SerializeField] string gorgeScene;
    [SerializeField] string mothershipScene;
    [SerializeField] string optionsMenu;
    [SerializeField] string creditsScene;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        EventSystem.current.SetSelectedGameObject(null);
    }


    public void Continue()
    {
        if (GameSession.instance.LoadGame())
            LevelLoader.instance.LoadLevel(GameSession.instance.Data.currentLevelIndex);
    }


    public void NewGame()
    {
        GameSession.instance.NewGame();
        LevelLoader.instance.LoadLevel(outpostScene); 
    }

    public void LoadGorge()
    {
        LevelLoader.instance.LoadLevel(gorgeScene);
    }
    public void LoadMothership()
    {
        LevelLoader.instance.LoadLevel(mothershipScene);
    }
    public void LoadOutpost()
    {   
        LevelLoader.instance.LoadLevel(outpostScene);
    }

    public void LoadCredits()
    {
        LevelLoader.instance.LoadLevel(creditsScene);
    }
    public void Quit()
    {
        Application.Quit();
    }

    public void OptionsMenu()
    {
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
        LevelLoader.instance.LoadLevel(optionsMenu);
    }

    public void OnContinue()
    {
        if(GameSession.instance.LoadGame())
        {
            LevelLoader.instance.LoadLevel(GameSession.instance.Data.currentLevelIndex);
        }
    }
}
