using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;


public class OptionsMenu : MonoBehaviour
{
    [SerializeField] int mainMenu = 1;

    [SerializeField] GameObject optionsMenuFirstButton;
    [SerializeField] GameObject activeMenu;
    [SerializeField] GameObject videoMenu;
    [SerializeField] GameObject videoMenuFirstButton;
    [SerializeField] GameObject controlsMenu;
    [SerializeField] GameObject controlsMenuFirstButton;
    [SerializeField] GameObject audioMenu;
    [SerializeField] GameObject audioMenuFirstButton;


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
        
    }

    public void ExitToMainMenu()
    {
        SceneManager.LoadScene(mainMenu);
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


    public void Audio()
    {
        if (activeMenu != null)
        {
        UnloadMenu(activeMenu);
            
        }
        newMenu(audioMenu, audioMenuFirstButton);
        
    }
}
