using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class DroneWorldUI : MonoBehaviour
{

    [SerializeField] Image progressRing;
    [SerializeField] TMP_Text hintText;

    [Header("Billboard")]
    [SerializeField] bool faceCamera = true;
    Camera cam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        cam = Camera.main;
        SetProgress(0f);
        ClearHint();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!faceCamera) return;
        if(!cam) cam = Camera.main;
        if (!cam) return;

        transform.forward = cam.transform.forward; //billboard
    }

    public void ClearHint()
    {
        if (!hintText) return;
        hintText.text = "";
        hintText.gameObject.SetActive(false);
    }
    
    public void SetHint(string msg)
    {
        if (!hintText) return;
        hintText.text = msg;
        hintText.gameObject.SetActive(!string.IsNullOrEmpty(msg));
    }

    public void SetProgress(float v)
    {
        if (!progressRing) return;
        progressRing.fillAmount = Mathf.Clamp01(v);
        progressRing.gameObject.SetActive(v > 0.001f);
    }

    
}
