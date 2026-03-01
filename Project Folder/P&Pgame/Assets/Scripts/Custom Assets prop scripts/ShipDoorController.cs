using System;
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

    Coroutine routine;

    public bool isOpen { get; private set; }


    private void Awake()
    {
        if (!leftDoor || !rightDoor)
            Debug.LogWarning($"{name}: Assign leftDoor/rightDoor", this);

        leftStart = leftDoor.localPosition;
        rightStart = rightDoor.localPosition;
    }
    void Start()
    {
        
    }

    public void Open()
    {
        if (isOpen)
            return;
        StartMove(OpenRoutine());
        isOpen = true;
    }
    public void Close()
    {
        if (!isOpen)
            return;
        StartMove(CloseRoutine());
        isOpen = false;
    }

    void StartMove(IEnumerator r)
    {
        if (routine != null) 
            StopCoroutine(routine);
        routine = StartCoroutine(r);
    }

    IEnumerator OpenRoutine()
    {
        if(aud && pullInClip) aud.PlayOneShot(pullInClip, pullinClipVol);
        yield return new WaitForSeconds(0.4f);

        // phase 1 - Push in (local z negative)
        yield return MoveDoors(
            leftStart, rightStart,
            leftStart + new Vector3(0, 0, -pushInDistance),
            rightStart + new Vector3(0, 0, -pushInDistance),
            playSlideSfx: false);

        // phase 2 - Slide apart
        if(aud && slideClip) aud.PlayOneShot(slideClip, slideClipVol);

        yield return MoveDoors(
            leftStart + new Vector3(0, 0, -pushInDistance),
            rightStart + new Vector3(0, 0, -pushInDistance),
            leftStart + new Vector3(slideDistance, 0, -pushInDistance),
            rightStart + new Vector3(-slideDistance, 0, -pushInDistance),
            playSlideSfx: false);
    }

    IEnumerator CloseRoutine()
    {
        
        // phase 1 - Slide together
        aud.PlayOneShot(slideClip, slideClipVol);
        yield return MoveDoors(
            leftStart + new Vector3(slideDistance, 0, -pushInDistance),
            rightStart + new Vector3(-slideDistance, 0, -pushInDistance),
            leftStart + new Vector3(0, 0, -pushInDistance),
            rightStart + new Vector3(0, 0, -pushInDistance),
            playSlideSfx: false);

        // phase 2 - Push out (lock)
        aud.PlayOneShot(pullInClip, pullinClipVol);
        

        yield return MoveDoors(
            leftStart + new Vector3(0, 0, -pushInDistance),
            rightStart + new Vector3(0, 0, -pushInDistance),
            leftStart, rightStart,
            playSlideSfx: false);

    }
        


    IEnumerator MoveDoors(Vector3 leftFrom, Vector3 rightFrom, Vector3 leftTo, Vector3 rightTo, bool playSlideSfx)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            float a = Mathf.SmoothStep(0, 1, t);

            leftDoor.localPosition = Vector3.Lerp(leftFrom, leftTo, a);
            rightDoor.localPosition = Vector3.Lerp(rightFrom, rightTo, a);

            yield return null;
        }

        leftDoor.localPosition = leftTo;
        rightDoor.localPosition = rightTo;
    }

}
