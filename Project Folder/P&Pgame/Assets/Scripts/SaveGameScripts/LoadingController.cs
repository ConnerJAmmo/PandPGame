using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    public GameObject loadingScreen;
    public TMP_Text progressText;
    public Slider slider;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(LoadAsync());
    }

    // from levelloader script
    IEnumerator LoadAsync() // we do not need the parameter in the method sceneloader handles that
    {
        //yield return null; // This is to let the UI render at least 1 frame first

        loadingScreen.SetActive(true);

        int target = SceneLoader.TargetSceneIndex; // this handles the scene index
        AsyncOperation operation = SceneManager.LoadSceneAsync(target);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            slider.value = progress;

            float progressPR = progress * 100;
            progressText.text = progressPR.ToString("F0") + "%";
            yield return null;
        }
    }
}


    
