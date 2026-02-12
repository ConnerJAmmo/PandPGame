using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonFunctions : MonoBehaviour
{
    public void Resume()
    {
        gameManager.instance.stateUnpause();
    }

    public void Restart()
    {
        if (gameManager.instance.playerScript.gunList.Count != 0)
        {
            gameManager.instance.playerScript.resetGunStatsToOrig();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            gameManager.instance.stateUnpause();   
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            gameManager.instance.stateUnpause();   
        }
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
            gameManager.instance.playerScript.resetGunStatsToOrig();
            Application.Quit(); 
        }
        else
        {
            gameManager.instance.playerScript.resetGunStatsToOrig();
            Application.Quit();  
        }

#endif
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
        if (gameManager.instance.fireRateLevel != gameManager.instance.maxLevel
            && gameManager.instance.goldCount >= gameManager.instance.fireRateUpgradeCost)
        {
            gameManager.instance.upgradePlayerShootRate();
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
}
