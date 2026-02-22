using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonFunctions : MonoBehaviour
{
    private static string lastSceneName;
    [SerializeField] int mainMenuScene;

    public void DebugRoom()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "DebugRoom")
        {
            // If we are already in DebugRoom, go back to where we came from
            if (!string.IsNullOrEmpty(lastSceneName))
            {
                SceneManager.LoadScene(lastSceneName);
            }
            else
            {
                //Debug.LogWarning("No previous scene recorded! Defaulting to Menu.");
                SceneManager.LoadScene("MainMenu"); // Fallback
            }
        }
        else
        {
            // If we are anywhere else, save the current scene and go to DebugRoom
            lastSceneName = currentScene;
            SceneManager.LoadScene("DebugRoom");
        }
        gameManager.instance.stateUnpause();
    }

    public void Resume()
    {
        gameManager.instance.stateUnpause();
    }

    public void Restart()
    {
        
        if (gameManager.instance.playerScript.gunList.Count != 0)
        {
            gameManager.instance.playerScript.resetGunStatsToOrig();
            gameManager.instance.CompleteLevelAndLoadNext(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            gameManager.instance.CompleteLevelAndLoadNext(SceneManager.GetActiveScene().buildIndex);
        }
        gameManager.instance.stateUnpause();
    }
    public void Quit()
    {
#if UNITY_EDITOR
        
        if (gameManager.instance.playerScript.gunList.Count != 0)
        {
            gameManager.instance.playerScript.resetGunStatsToOrig();
            UnityEditor.EditorApplication.isPlaying = false; 
        }
        else
        {
            UnityEditor.EditorApplication.isPlaying = false;   
        }
#else
        if (gameManager.instance.playerScript.gunList.Count != 0)
        {
            gameManager.instance.playerScript.resetGunStatsToOrig();
            Application.Quit(); 
        }
        else
        {
            Application.Quit();  
        }

#endif
    }

    public void Options()
    {
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
        SceneManager.LoadScene("OptionsMenu", LoadSceneMode.Additive);
    }

    public void UpgradeDamage()
    {
        if (gameManager.instance.damageLevel != gameManager.instance.maxLevel 
            && gameManager.instance.goldCount >= gameManager.instance.damageUpgradeCost)
        {
            gameManager.instance.upgradePlayerShootDamage();
        }
    }

    public void UpgradeFireRate()
    {
        if (gameManager.instance.maxAmmoLevel != gameManager.instance.maxLevel
            && gameManager.instance.goldCount >= gameManager.instance.maxAmmoUpgradeCost)
        {
            gameManager.instance.upgradePlayerMaxAmmo();
        }
    }

    public void UpgradeRange()
    {
        if (gameManager.instance.rangeLevel != gameManager.instance.maxLevel
            && gameManager.instance.goldCount >= gameManager.instance.rangeUpgradeCost)
        {
            gameManager.instance.upgradePlayerShootRange();
        }
    }

    public void mainMenu()
    {
        gameManager.instance.CompleteLevelAndLoadNext(mainMenuScene);
        gameManager.instance.stateUnpause();
    }

    public void nextLevel()
    {
        gameManager.instance.LevelComplete();
        gameManager.instance.stateUnpause();
    }
}
