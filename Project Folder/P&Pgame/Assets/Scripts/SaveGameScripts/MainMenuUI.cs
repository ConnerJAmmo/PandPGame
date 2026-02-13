using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
  
    public void Continue()
    {
        if (GameSession.instance.LoadGame())
            SceneManager.LoadScene(GameSession.instance.Data.currentLevelIndex);

    }

    public void NewGame()
    {
        GameSession.instance.NewGame();
        SceneManager.LoadScene(0); // if we have another scene at index 0, like Main menu, then this should be 1 or whenever the first play level start
    }

}
