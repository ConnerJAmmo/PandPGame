using UnityEngine;

public class cameraControler : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Transform player;

    [Header("Stats")]
    [Range(300, 3000)] [SerializeField] int sens;
    [Range(-180, 180)] [SerializeField] int lockVertMin, lockVertMax;
    [SerializeField] bool invertY;

    float camRotX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sens * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sens * Time.deltaTime;

        if (invertY)
        {
            camRotX += mouseY;
        }
        else 
        {
            camRotX -= mouseY;
        }

        camRotX = Mathf.Clamp(camRotX, lockVertMin, lockVertMax);

        transform.localRotation = Quaternion.Euler(camRotX,0,0);

        player.Rotate(Vector3.up * mouseX);
    }
}
