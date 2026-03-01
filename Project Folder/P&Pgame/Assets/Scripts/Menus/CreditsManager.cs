using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.IO;
using UnityEngine.UI;


public class CreditsManager : MonoBehaviour
{
    [SerializeField] string mainMenu;
    [SerializeField] string filePath;
    [SerializeField] TMP_Text tMP_Text;

    private void Start()
    {
        string textTo = File.ReadAllText(filePath);

        tMP_Text.text = textTo;
    }

    public void ExitToMain()
    {
        LevelLoader.instance.LoadLevel(mainMenu);
    }
}
