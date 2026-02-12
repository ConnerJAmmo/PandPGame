using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class TeleportPostFX : MonoBehaviour
{
    [SerializeField] Volume volume;
    [SerializeField] float flashIntensity = 0.06f;
    [SerializeField] float flashUpTime = 0.05f;
    [SerializeField] float flashDownTime = 0.12f;

    ChromaticAberration ca;

    private void Awake()
    {
        if (!volume) volume = FindAnyObjectByType<Volume>();
        if (volume && volume.profile)
        {
            volume.profile.TryGet(out ca);
        }
    }

    public void Flash()
    {
        if (ca == null)
            return;
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
        
    }

    IEnumerator FlashRoutine()
    {
        float baseVal = ca.intensity.value;

        //ramp Up
        for (float t = 0; t < flashUpTime; t += Time.unscaledDeltaTime)
        {
            ca.intensity.value = Mathf.Lerp(baseVal, flashIntensity, t / flashUpTime);
            yield return null;
        }
        
        for (float t = 0; t < flashDownTime; t += Time.unscaledDeltaTime)
        {
            ca.intensity.value = Mathf.Lerp(baseVal, flashIntensity, t / flashDownTime);
            yield return null;
        }

        ca.intensity.value = baseVal;
    }
}
