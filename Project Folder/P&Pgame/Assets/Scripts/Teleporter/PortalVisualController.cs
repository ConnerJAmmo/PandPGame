using System.Collections;
using UnityEngine;

public class PortalVisualController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] MeshRenderer portalSurfaceRenderer;
    [SerializeField] Camera portalCamera;

    [Header("Shader Property")]
    [SerializeField] string fadeProp = "_Fade";

    [Header("Fade Settings")]
    [SerializeField] float fadeInTime = 0.5f;
    [SerializeField] float fadeOutTime = 0.25f;

    MaterialPropertyBlock matPB;

    Coroutine fadeRoutine;
    float fade = 0f;

    private void Awake()
    {
        matPB = new MaterialPropertyBlock();
        
        // keep renderer enable so it exist, just Transparent
        if (portalSurfaceRenderer) portalSurfaceRenderer.enabled = true;

        SetFade(0f);


        if (portalCamera) portalCamera.enabled = false;
    }

    public void TurnOn()
    {
        if (portalSurfaceRenderer) portalSurfaceRenderer.enabled = true;
        if (portalCamera) portalCamera.enabled = true;
       
        StartFade(1f, fadeInTime, disableCamAfter: true);


    }

    public void TurnOff()
    {
        StartFade(0f, fadeOutTime, disableCamAfter: true);
        
    }

    void StartFade(float target, float time,bool disableCamAfter)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = StartCoroutine(FadeTo(target, time, disableCamAfter));
    }

    IEnumerator FadeTo(float target, float time, bool disableCamAfter)
    {
        float start = fade;

        if (time <= 0f)
            SetFade(target);
        else
        {   float t = 0f;

             while (t < time)
             {
                t += Time.deltaTime;
                float a = Mathf.Clamp01(t / time);
                SetFade(Mathf.Lerp(start, target, a));
                yield return null;
             }

            SetFade(target);
        }

        if (disableCamAfter && Mathf.Approximately(target,0f))
        {
            if (portalCamera) portalCamera.enabled = false;

            if (portalSurfaceRenderer) portalSurfaceRenderer.enabled = false;
        }

        fadeRoutine = null;
    }

        
    void SetFade(float v)
    {
        fade = Mathf.Clamp01(v);

        if (!portalSurfaceRenderer) return;

        portalSurfaceRenderer.GetPropertyBlock(matPB);
        matPB.SetFloat(fadeProp, fade);
        portalSurfaceRenderer.SetPropertyBlock(matPB);
    }



}
