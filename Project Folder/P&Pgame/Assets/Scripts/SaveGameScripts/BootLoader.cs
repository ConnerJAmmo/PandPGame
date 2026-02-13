using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    [SerializeField] int mainMenuSceneIndex = 1;

    void Start()
    {
        // Ensure a session exist (if we ever run a gameplay scene directly)
        if (!GameSession.instance)
        {
            var go = new GameObject("GameSession");
            go.AddComponent<GameSession>();
        }

        SceneManager.LoadScene(mainMenuSceneIndex);
    }
}
