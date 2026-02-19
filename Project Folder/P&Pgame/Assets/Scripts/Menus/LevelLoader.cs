using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Xml.Serialization;
using NUnit.Framework;


public class LevelLoader : MonoBehaviour
{
    public static LevelLoader instance;

    public GameObject loadingScreen; 
    [SerializeField] GameObject outpostImage;
    [SerializeField] GameObject gorgeImage;
    [SerializeField] GameObject mothershipImage;
    [SerializeField] GameObject backgroundImage;
    public Slider slider;
    public TMP_Text progressText;

    [SerializeField] string mothershipStringScene;
    [SerializeField] int mothershipIntScene;
    [SerializeField] string gorgeStringScene;
    [SerializeField] int gorgeIntScene;
    [SerializeField] string outpostStringScene;
    [SerializeField] int outpostIntScene;
    private void Start()
    {
        instance = this;
    }

    public void LoadLevel (string sceneIndex)
    {
        StartCoroutine(LoadAsynchronously(sceneIndex));
    } 

    IEnumerator LoadAsynchronously (string sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        
        loadingScreen.SetActive(true);

        if (sceneIndex == mothershipStringScene)
        {
            mothershipImage.SetActive(true);
        }
        else if (sceneIndex == outpostStringScene)
        {
            outpostImage.SetActive(true);
        }
        else if (sceneIndex == gorgeStringScene)
        {
            gorgeImage.SetActive(true);
        }
        else
        {
            backgroundImage.SetActive(true);
        }

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            slider.value = progress;

            float progressPr = progress * 100;
            progressText.text = progressPr.ToString("F0") + "%";

            yield return null;
        }
    }

    public void LoadLevel (int sceneIndex)
    {
        StartCoroutine(LoadAsynchronously(sceneIndex));
    } 

    IEnumerator LoadAsynchronously (int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        loadingScreen.SetActive(true);

        if (sceneIndex == mothershipIntScene)
        {
            mothershipImage.SetActive(true);
        }
        else if (sceneIndex == outpostIntScene)
        {
            outpostImage.SetActive(true);
        }
        else if (sceneIndex == gorgeIntScene)
        {
            gorgeImage.SetActive(true);
        }
        else
        {
            backgroundImage.SetActive(true);
        }

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            slider.value = progress;

            float progressPr = progress * 100;
            progressText.text = progressPr.ToString("F0") + "%";

            yield return null;
        }
    }
}
