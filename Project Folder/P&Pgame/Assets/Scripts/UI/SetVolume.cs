using UnityEngine;
using UnityEngine.Audio;



public class SetVolume : MonoBehaviour
{
    [SerializeField] bool isSFX;
    [SerializeField] AudioSource testSource;

    [SerializeField] AudioClip[] testAud;

    public AudioMixer mixer;


    public void SetLevel (float sliderValue)
    {
        if (isSFX)
        {
            mixer.SetFloat("SFXVol", Mathf.Log10(sliderValue) * 20);
            testSource.PlayOneShot(testAud[0], sliderValue);

        }
        else
        {
            mixer.SetFloat("MusicVol", Mathf.Log10(sliderValue) * 20);
            
        }

    }


    
}
