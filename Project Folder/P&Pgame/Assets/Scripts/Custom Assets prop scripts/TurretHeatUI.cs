using UnityEngine;
using UnityEngine.UI;

public class TurretHeatUI : MonoBehaviour
{
    [SerializeField] Image fill;
    [SerializeField] Color coolColor = Color.cyan;
    [SerializeField] Color hotColor = Color.red;

    Camera cam;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(!cam) cam = Camera.main;
        if(cam) transform.forward = cam.transform.forward;
    }

    public void SetHeat01(float t)
    {
        t = Mathf.Clamp01(t);
        if(fill) fill.fillAmount = t;

        //Color shift
        if(fill) fill.color = Color.Lerp(coolColor, hotColor, t);
    }
}
