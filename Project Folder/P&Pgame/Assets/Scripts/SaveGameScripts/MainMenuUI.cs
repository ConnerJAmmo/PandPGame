using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Gameplay Scene Indexes")]

    [SerializeField] int outpostScene = 3;
    [SerializeField] int gorgeScene = 4;
    [SerializeField] int mothershipScene = 5;
    [SerializeField] int optionsMenu = 6;
    [SerializeField] int creditsScene = 7;


  
    public void Continue()
    {
        if (GameSession.instance.LoadGame())
            SceneManager.LoadScene(GameSession.instance.Data.currentLevelIndex);
    }


    public void NewGame()
    {
        GameSession.instance.NewGame();
        SceneManager.LoadScene(outpostScene); // if we have another scene at index 0, like Main menu, then this should be 1 or whenever the first play level start
    }

    public void LoadGorge()
    {
        SceneLoader.load(gorgeScene);
    }
    public void LoadMothership()
    {
        SceneLoader.load(mothershipScene);
    }
    public void LoadOutpost()
    {
        SceneLoader.load(outpostScene);
    }

    public void LoadCredits()
    {
        SceneLoader.load(creditsScene);
    }
    public void Quit()
    {
        Application.Quit();
    }

    public void OptionsMenu()
    {
        SceneLoader.load(optionsMenu);
    }

    public void OnContinue()
    {
        if(GameSession.instance.LoadGame())
        {
            SceneManager.LoadScene(GameSession.instance.Data.currentLevelIndex);
        }
    }
}
