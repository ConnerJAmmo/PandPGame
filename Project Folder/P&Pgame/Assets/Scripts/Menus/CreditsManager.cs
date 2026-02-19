using UnityEngine;
using UnityEngine.SceneManagement;


public class CreditsManager : MonoBehaviour
{
    [SerializeField] string mainMenu;


    public void ExitToMain()
    {
        LevelLoader.instance.LoadLevel(mainMenu);
    }
}
