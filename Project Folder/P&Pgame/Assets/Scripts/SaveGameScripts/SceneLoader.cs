using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static int TargetSceneIndex { get; private set; }

    public static void load(int tarSceneIndex)
    {
        TargetSceneIndex = tarSceneIndex;
        SceneManager.LoadScene(2); // !!TEAM, CHANGE THIS!! if loading screen is not in index 2 
    }
}
