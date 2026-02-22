using System;
using UnityEngine;

public class ElectricLine : MonoBehaviour
{
    [SerializeField] Renderer rend;
    [SerializeField] string powerPrep = "_Power";
    [SerializeField] float onPower = 1f;
    [SerializeField] float offPower = 0f;
    [SerializeField] float fadeTime = 0.25f;

    MaterialPropertyBlock mpb;
    float current;
    float target;
    float vel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (!rend) 
            rend = GetComponentInChildren<Renderer>();
        mpb = new MaterialPropertyBlock();
        SetPower(offPower);
    }

    public void TurnOn() => target = onPower;
    public void TurnOff() => target = offPower;

    private void Update()
    {
        // SmoothDAmp poer for a nice "energize" ramp
        current = Mathf.SmoothDamp(current, target, ref vel, fadeTime);
        SetPower(current);
    }
    private void SetPower(float v)
    {
        if (!rend) return;

        rend.GetPropertyBlock(mpb);
        mpb.SetFloat(powerPrep, v);
        rend.SetPropertyBlock(mpb);
    }


    
}
