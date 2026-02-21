using UnityEngine;

public class KeyCardRotate : MonoBehaviour
{
    [Header("Bob")]
    [SerializeField] bool useLocalSpace = false; // This is for the team if you want to use the script and the object is parented
    [SerializeField] float rotateSpeed = 1.0f;
    [SerializeField] float bobHeight = 1.0f;

    [Header("Rotate")]
    [SerializeField] Vector3 rotateDegreePerSec = new Vector3(0f, 90, 0f);

    [Header("Offset (optional")]
    [SerializeField] bool randomStartOffset = true;

    Vector3 startPos;
    float tOffset;
    bool running = true;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (useLocalSpace)
            startPos = transform.localPosition;
        else
            startPos = transform.position;

        if (randomStartOffset)
            tOffset = Random.Range(0f, 1000f);
        else
            tOffset = 0f;

    }

    // Update is called once per frame
    void Update()
    {
        if (!running) return;

        //bob
        float y = Mathf.Sin((Time.time + tOffset) * bobHeight);
        if (useLocalSpace)
            transform.localPosition = startPos + new Vector3(0f, y, 0f);
        else
            transform.position = startPos + new Vector3(0f, y, 0f);

        //rotate
        transform.Rotate(rotateDegreePerSec * Time.deltaTime, Space.Self);

    }

    public void Stop(bool resetToStartPos = false)
    {
        running = false;
        if (resetToStartPos)
        {
            if (useLocalSpace)
                transform.localPosition = startPos;
            else
                transform.position = startPos;
        }
    }

    // if we move it at runtime and want the bob center to update
    public void Recenter()
    {
        if(useLocalSpace)
            startPos = transform.localPosition;
        else
            startPos = transform.position;
    }
}
