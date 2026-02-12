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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gameManager.instance.playerScript.resetGunStatsToOrig();
        gameManager.instance.stateUnpause();
    }
    public void Quit()
    {
#if UNITY_EDITOR
        gameManager.instance.playerScript.resetGunStatsToOrig();
        UnityEditor.EditorApplication.isPlaying = false;
#else
    gameManager.instance.playerScript.resetGunStatsToOrig();
    Application.Quit();

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
