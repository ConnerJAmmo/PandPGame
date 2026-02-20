using System.Collections;
using UnityEngine;

public class ShipDoorController : MonoBehaviour
{

    [Header("Door Parts")]
    [SerializeField] Transform leftDoor;
    [SerializeField] Transform rightDoor;

    [Header("Movement")]
    [SerializeField] float pushInDistance = 0.3f;
    [SerializeField] float slideDistance = 1.8f;
    [SerializeField] float moveSpeed = 2f;

    [Header("Audio")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip pullInClip;
    [SerializeField] AudioClip slideClip;
    [Range(0, 1)][SerializeField] float slideClipVol;
    [Range(0, 1)][SerializeField] float pullinClipVol;
    Vector3 leftStart;
    Vector3 rightStart;

    bool isOpen;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        leftStart = leftDoor.localPosition;
        rightStart = rightDoor.localPosition;
    }

    public void Open()
    {
        if (isOpen)
            return;
        StartCoroutine(OpenRoutine());
        isOpen = true;
    }

    IEnumerator OpenRoutine()
    {
        aud.PlayOneShot(pullInClip, pullinClipVol);
        yield return new WaitForSeconds(0.4f);

        aud.PlayOneShot(slideClip, slideClipVol);
        
        // phase 1 - Push in (local z negative)
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * moveSpeed;
            float a = Mathf.SmoothStep(0, 1, t);

            leftDoor.localPosition = leftStart + new Vector3(0, 0, -pushInDistance * a);
            rightDoor.localPosition = rightStart + new Vector3(0, 0, -pushInDistance * a);
            
            yield return null;
        }

        // phase 2 - Slide apart
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * moveSpeed;
            float a = Mathf.SmoothStep(0, 1, t);

            leftDoor.localPosition = leftStart + new Vector3(slideDistance * a, 0, -pushInDistance);

            rightDoor.localPosition = rightStart + new Vector3(-slideDistance * a, 0, -pushInDistance);
            
            yield return null;
        }
    }

}
