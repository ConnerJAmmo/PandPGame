using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.UIElements.Experimental;



public class OptionsMenu : MonoBehaviour
{
    
    [SerializeField] string mainMenu;
    string previousScene;
    public bool isSaved;

    [SerializeField] GameObject notSavedMessage;
    [SerializeField] GameObject notSavedMessageFristButton;
    [SerializeField] GameObject mainMenuExitButton;
    [SerializeField] GameObject backToLevelButton;
    [SerializeField] GameObject optionsMenuFirstButton;
    [SerializeField] GameObject activeMenu;
    [SerializeField] GameObject videoMenu;
    [SerializeField] GameObject videoMenuFirstButton;
    [SerializeField] GameObject controlsMenu;
    [SerializeField] GameObject controlsMenuFirstButton;
    [SerializeField] GameObject audioMenu;
    [SerializeField] GameObject audioMenuFirstButton;
#region Audio Variables
    float masterVol, masterVolText, SFXVol, SFXVolText, musicVol, musicVolText;
    [SerializeField] TMP_Text masterText, SFXText, musicText;
    public AudioMixer mixer;
    #endregion

    void Start()
    {
        previousScene = PlayerPrefs.GetString("PreviousScene");
        isSaved = true;

        if (previousScene == "MainMenu")
        {  
            mainMenuExitButton.SetActive(true);
        }
        else
        {
            backToLevelButton.SetActive(true);
        }

        StartAudio();
    }

    void Update()
    {
        
    }

    public void newMenu(GameObject menu, GameObject firstButton)
    {
        
        activeMenu = menu;
        activeMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstButton);
    }

    public void UnloadMenu(GameObject menu)
    {
        
        activeMenu = menu;
        activeMenu.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void Save()
    {
        isSaved = true;
    }

#region NotSavedMessage
    public void NotSavedMessage()
    {
        
        notSavedMessage.SetActive(true);
        
    } 

    public void NotSavedMessageSavedButton()
    {
        Save();
        if (previousScene == "MainMenu")
        {
            ExitToMainMenu();
        }
        else
        {
            BackToGame();
        }
    }

    public void NotSavedMessageDontSavedButton()
    {   
        if (previousScene == "MainMenu")
        {
            ExitToMainMenu();
        }
        else
        {
            BackToGame();
        }
    }

    public void NotSavedMessageCancelButton()
    {
        notSavedMessage.SetActive(false);
    }

#endregion


    public void ExitToMainMenu()
    {
        if (isSaved == false)
        {
            NotSavedMessage();
        }
        else if (isSaved == true)
        {
            LevelLoader.instance.LoadLevel(mainMenu);   
        }
    }

    public void BackToGame()
    {
        if (isSaved == false)
        {
            NotSavedMessage();
        }
        else if (isSaved == true)
        {
            gameManager.instance.isOptionsOpen = false;
            SceneManager.UnloadSceneAsync("OptionsMenu");   
        }
    }

    public void Video()
    {
        if (activeMenu != null)
        {
            UnloadMenu(activeMenu);    
        }
            newMenu(videoMenu, videoMenuFirstButton);

    }

    public void Controls()
    {
        if (activeMenu != null)
        {
            UnloadMenu(activeMenu);
            
        }
            newMenu(controlsMenu, controlsMenuFirstButton);
        
    }

#region Audio
    public void Audio()
    {
        if (activeMenu != null)
        {
            UnloadMenu(activeMenu);
            
        }
            newMenu(audioMenu, audioMenuFirstButton);
        
    } 

    public void StartAudio()
    {
 
       masterVol = Mathf.Log10(PlayerPrefs.GetFloat("Master", 1) * 20);
       masterVolText = PlayerPrefs.GetFloat("Master", 1) * 100;
       masterText.SetText(masterVolText.ToString("F0") + "%");
       SFXVol = Mathf.Log10(PlayerPrefs.GetFloat("SFXVol", 1) * 20);
       SFXVolText = PlayerPrefs.GetFloat("SFXVol", 1) * 100;
       SFXText.SetText(SFXVolText.ToString("F0") + "%");
       musicVol = Mathf.Log10(PlayerPrefs.GetFloat("MusicVol", 1) * 20);
       musicVolText = PlayerPrefs.GetFloat("MusicVol", 1) * 100;
       musicText.SetText(musicVolText.ToString("F0") + "%");
    }

    public void SetMasterLevel (float sliderValue)
    {
        masterVolText = sliderValue * 100;
        masterText.SetText($"{masterVolText.ToString("N0") + "%"}");
        masterVol = sliderValue;
        mixer.SetFloat("Master", Mathf.Log10(masterVol) * 20);
        PlayerPrefs.SetFloat("MasterVol", masterVol);
    }

    public void SetSFXLevel (float sliderValue)
    {
        SFXVolText = sliderValue * 100;
        SFXText.SetText($"{SFXVolText.ToString("N0") + "%"}");
        SFXVol = sliderValue;
        mixer.SetFloat("SFXVol", Mathf.Log10(SFXVol) * 20);
        PlayerPrefs.SetFloat("SFXVol", SFXVol);
    }

    public void SetMusicLevel(float sliderValue)
    {
        musicVolText = sliderValue * 100;
        musicText.SetText($"{musicVolText.ToString("N0") + "%"}");
        musicVol = sliderValue;
        mixer.SetFloat("MusicVol", Mathf.Log10(musicVol) * 20);
        PlayerPrefs.SetFloat("MusicVol", musicVol);
    }
#endregion
}
