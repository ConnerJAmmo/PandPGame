using Unity.VisualScripting;
using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData instance;
    [Header("Persistent Player Stats")]
    public int PlayerSpeedBoost;
    public int PlayerJumpBoost;
    public float PlayerMiningSpeedBoost;



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
        PlayerJumpBoost = 0;
        PlayerMiningSpeedBoost = 0;
    }

}
