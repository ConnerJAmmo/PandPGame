using System.Collections;
using System.Xml.Serialization;
using UnityEngine;

public class SimpleCameraShake : MonoBehaviour
{

    [SerializeField] Transform cam;
    [SerializeField] float duration = 0.12f;
    [SerializeField] float strength = 0.08f;

    Vector3 originalLocalPos;

    private void Awake()
    {
        if (!cam) cam = Camera.main.transform;
        originalLocalPos = cam.localPosition;
    }

    public void Shake()
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine());

    }

    IEnumerator ShakeRoutine()
    {
        float shakeTime = 0f;

        while(shakeTime < duration)
        {
            shakeTime += Time.deltaTime;
            cam.localPosition = originalLocalPos + Random.insideUnitSphere * strength;
            yield return null;
        }
        cam.localPosition = originalLocalPos;
    }

}
