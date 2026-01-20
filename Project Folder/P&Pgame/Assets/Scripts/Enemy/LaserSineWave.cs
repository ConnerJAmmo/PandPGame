using UnityEngine;

public class TrailPulse : MonoBehaviour
{
    private TrailRenderer trail;
    private AnimationCurve workingCurve;

    [Header("Pattern Settings")]
    public bool pulseIndividualKeys = false; // Toggle for key-specific control
    public float frequency = 5.0f;
    public float minWidth = 0.1f;
    public float maxWidth = 1.0f;

    void Start()
    {
        trail = GetComponent<TrailRenderer>();
        // Cache the curve to avoid creating new objects every frame
        workingCurve = trail.widthCurve;
    }

    void Update()
    {
        float wave = (Mathf.Sin(Time.time * frequency) + 1f) / 2f;
        float currentWidth = Mathf.Lerp(minWidth, maxWidth, wave);

        if (!pulseIndividualKeys)
        {
            // Original logic: Scaler for the whole trail
            trail.widthMultiplier = currentWidth;
        }
        else
        {
            // Advanced logic: Modify specific keys in the curve
            ModifyCurveKeys(currentWidth);
        }
    }

    void ModifyCurveKeys(float newValue)
    {
        // 1. Get a copy of the current keys
        Keyframe[] keys = workingCurve.keys;

        if (keys.Length > 0)
        {
            // Example: Only change the very first key (the "head" of the trail)
            keys[0].value = newValue;

            // Example: Only change the last key (the "tail")
            // keys[keys.Length - 1].value = newValue;
        }

        // 2. Reassign keys back to the working curve
        workingCurve.keys = keys;

        // 3. Reassign the curve back to the TrailRenderer to update it
        trail.widthCurve = workingCurve;
    }
}
