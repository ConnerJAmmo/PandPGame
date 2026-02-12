using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
   public static GameSession instance
    {
        get; private set; 
    }

    public SaveData Data { get; private set; } = new SaveData();

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void NewGame()
    {
        Data = new SaveData();
        Data.currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        SaveSystem.Save(Data);
    }

    public bool LoadGame()
    {
        if (SaveSystem.TryLoad(out var loaded))
        {
            Data = loaded;
            return true;
        }
        return false;
    }

    public void SaveGAme()
    {
        Data.currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        SaveSystem.Save(Data);
    }

}
