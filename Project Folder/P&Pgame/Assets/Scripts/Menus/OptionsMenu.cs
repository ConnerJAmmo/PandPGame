using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Audio;
using UnityEngine.UI;


public class OptionsMenu : MonoBehaviour
{
    [SerializeField] string mainMenu;
    string previousScene;
    bool isSaved;

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
    [SerializeField] bool isSFX;
    [SerializeField] AudioSource testSource;

    [SerializeField] AudioClip[] testAud;
    float masterValue, musicSliderValue, SFXsilderValue;
    public Slider masterSlider, musicSlider, SFXsilder;
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
    }

    void Update()
    {
        UpdateAudio();
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
        SaveAudioSettings();
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
        isSaved = true;
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
        else
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
        else
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
            StartAudio();
        
    } 

    public void StartAudio()
    {   
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume");
        SFXsilder.value = PlayerPrefs.GetFloat("SFXVolume");
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
    }



    public void UpdateAudio()
    {
        mixer.SetFloat("Master", masterValue);
        mixer.SetFloat("SFXVol", SFXsilderValue);
        mixer.SetFloat("MusicVol", musicSliderValue);
        isSaved = false;
    }

    public void SetMaterLevel (float sliderValue)
    {
        masterValue = sliderValue;

    }

    public void SetSFXLevel (float sliderValue)
    {
        SFXsilderValue = sliderValue;

    }

    public void SetMusicLevel(float sliderValue)
    {
        musicSliderValue = sliderValue;
    }

    public void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterValue);
        PlayerPrefs.SetFloat("SFXVolume", SFXsilderValue);
        PlayerPrefs.SetFloat("MusicVolume", musicSliderValue);
        PlayerPrefs.Save();
    }
#endregion
}
