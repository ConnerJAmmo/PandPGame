using UnityEngine;

public class EmissivePulse : MonoBehaviour
{

    [SerializeField] Renderer r;
    [SerializeField] Color emissiveColor = Color.cyan;
    [SerializeField] float intensityMin = 0.5f;
    [SerializeField] float intensityMax = 3.5f;
    [SerializeField] float speed = 2f;

    MaterialPropertyBlock mpb;
    static readonly int EmissiveId = Shader.PropertyToID("_EmissionColor");

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (!r) r = GetComponentInChildren<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    // Update is called once per frame
    void Update()
    {
        if (!r) return;

        float t = (Mathf.Sin(Time.time * speed) * 0.5f) + 0.5f;
        float intensity = Mathf.Lerp(intensityMin, intensityMax, t);

        r.GetPropertyBlock(mpb);
        mpb.SetColor(EmissiveId, emissiveColor * intensity);
        r.SetPropertyBlock(mpb);
    }
}
