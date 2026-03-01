using UnityEngine;
using UnityEngine.Audio;



public class SetVolume : MonoBehaviour
{
    public static SetVolume instance;
    
    [SerializeField] bool isSFX;
    [SerializeField] AudioSource testSource;

    [SerializeField] AudioClip[] testAud;
    public float masterValue, musicSliderValue, SFXsilderValue;
    public AudioMixer mixer;

    void Start()
    {
        PlayerPrefs.GetFloat("SFX Volume", SFXsilderValue);
        PlayerPrefs.GetFloat("Music Volume", musicSliderValue);
    }

    void Update()
    {
        mixer.SetFloat("SFXVol", Mathf.Log10(SFXsilderValue) * 20);
        PlayerPrefs.SetFloat("SFX Volume", SFXsilderValue);
        mixer.SetFloat("MusicVol", Mathf.Log10(musicSliderValue) * 20);
        PlayerPrefs.SetFloat("Music Volume", musicSliderValue);
    }

    public void SetLevel (float sliderValue)
    {
        if (isSFX)
        {
            SFXsilderValue = sliderValue;
        }
        else
        {
            musicSliderValue = sliderValue;
        }

    }

    public void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("SFX Volume", SFXsilderValue);
        PlayerPrefs.SetFloat("Music Volume", musicSliderValue);
    }
}
