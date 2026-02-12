using Unity.VisualScripting;
using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData instance;
    [Header("Persistent Player Stats")]
    public int PlayerSpeedBoost;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetAllData()
    {
        PlayerSpeedBoost = 0;
    }

}
