using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class cameraContr : MonoBehaviour
{
    [SerializeField] int sens;
    [SerializeField] int lockVertMin, lockVertMax;
    [SerializeField] bool invertY;
    [SerializeField] Transform player;

    [Header("--- FOV ---")]
    [SerializeField] Camera cam;
    [SerializeField] float defaultFOV = 60f;
    [SerializeField] float sprintFOV = 80f;
    [SerializeField] float fovChangeSpeed = 10f;
    [Header("--- Wall Jump Tilt ---")]
    [SerializeField] float wallJumpTiltAngle = 15f;
    [SerializeField] float tiltSpeed = 8f;

    float camRotX;
    float targetFOV;
    float targetTilt = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        EventSystem.current.SetSelectedGameObject(null);
        targetFOV = defaultFOV;
        cam.fieldOfView = defaultFOV;

    }

    // Update is called once per frame
    void Update()
    {
        
        float mouseX = Input.GetAxisRaw("Mouse X") * sens * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sens * Time.deltaTime;;

        if(invertY)
        {
            camRotX += mouseY;
        }
        else
        {
            camRotX -= mouseY;
        }

        camRotX = Mathf.Clamp(camRotX, lockVertMin, lockVertMax);

        float currentTilt = Mathf.LerpAngle(transform.localRotation.eulerAngles.z, targetTilt, tiltSpeed * Time.deltaTime);

        transform.localRotation = Quaternion.Euler(camRotX, 0, currentTilt);

        player.Rotate(Vector3.up * mouseX);

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovChangeSpeed * Time.deltaTime);
    }

    public void SetSprintFOV()
    {
        targetFOV = sprintFOV;
    }

    public void ResetFOV()
    {
        targetFOV = defaultFOV;
    }

    public void SetWallJumpTilt(float direction)
    {
        targetTilt = wallJumpTiltAngle * direction;
    }

    public void ResetTilt()
    {
        targetTilt = 0f;
    }
}