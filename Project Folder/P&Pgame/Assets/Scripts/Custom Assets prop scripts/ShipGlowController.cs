using JetBrains.Annotations;
using System;
using UnityEngine;

public class ShipGlowController : MonoBehaviour
{
    [SerializeField] Renderer[] renderersWithGreenMat;
    [ColorUsage(true, true)]
    [SerializeField] Color emissionColor = Color.green;
    [SerializeField] float emissionIntensityOff = 0f;
    [SerializeField] float emissionIntensityOn = 8f;

    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    MaterialPropertyBlock mpb;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        mpb = new MaterialPropertyBlock();
        SetGlow(false);
    }

    public void SetGlow(bool on)
    {
        float intensity;
        if (on)
            intensity = emissionIntensityOn;
        else
            intensity = emissionIntensityOff;

        Color final = emissionColor * intensity;

        foreach (var r in renderersWithGreenMat)
        {
            if (!r) continue;
            r.GetPropertyBlock(mpb);
            mpb.SetColor(EmissionColorId, final);
            r.SetPropertyBlock(mpb);
        }
    }
}
