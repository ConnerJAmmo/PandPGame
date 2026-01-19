using UnityEngine;

public class TrailPulse : MonoBehaviour
{
    private TrailRenderer trail;

    [Header("Sine Settings")]
    public float minWidth = 0.1f;
    public float maxWidth = 1.0f;
    public float frequency = 5.0f;

    void Start()
    {
        trail = GetComponent<TrailRenderer>();
    }

    void Update()
    {
        // Calculate sine wave value between 0 and 1
        float wave = (Mathf.Sin(Time.time * frequency) + 1f) / 2f;

        // Apply the wave to the width multiplier
        trail.widthMultiplier = Mathf.Lerp(minWidth, maxWidth, wave);
    }
}
