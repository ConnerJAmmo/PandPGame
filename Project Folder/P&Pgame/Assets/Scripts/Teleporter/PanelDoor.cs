using UnityEngine;

public class PanelDoor : MonoBehaviour
{
    [Header("Rotation (local)")]
    [SerializeField] Vector3 closedEuler = Vector3.zero;
    [SerializeField] Vector3 openEuler = new Vector3(90f, 0f, 0f);
    [SerializeField] float speed = 6f;

    float t; // 0 closed, 1 open
    bool isOpen;

    public void Open() => isOpen = true;
    public void Close() => isOpen = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        t = 0f;
        transform.localRotation = Quaternion.Euler(closedEuler);
    }

    // Update is called once per frame
    void Update()
    {
        float target = isOpen ? 1f : 0f;
        t = Mathf.MoveTowards(t, target, speed * Time.deltaTime);

        Quaternion rot = Quaternion.Slerp(Quaternion.Euler(closedEuler), Quaternion.Euler(openEuler), t);

        transform.localRotation = rot;
    }
}
