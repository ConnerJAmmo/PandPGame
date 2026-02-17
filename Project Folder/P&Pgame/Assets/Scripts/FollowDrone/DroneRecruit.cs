using UnityEngine;

public class DroneRecruit : MonoBehaviour
{

    [SerializeField] DroneController drone;
    [SerializeField] float holdTime = 1.5f;
    [SerializeField] KeyCode recruitKey = KeyCode.G;

    float held;
    bool playerInRange;

    Transform player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Reset()
    {
        if (!drone) drone = GetComponentInParent<DroneController>();

    }

    // Update is called once per frame
    void Update()
    {
        if (!player || !drone || drone.IsRecruited) return;


        drone.SetHint("Hold G to Activate drone");

        if (Input.GetKey(recruitKey))
        {
            held += Time.deltaTime;
            drone.SetProgress(held / holdTime);

            if (held >= holdTime)
            {
                drone.Recruit(player);
                held = 0;
                drone.SetProgress(0f);
            }
        }
        else
        {
            held = Mathf.MoveTowards(held, 0f, Time.deltaTime * 3f);
            drone.SetProgress(held / holdTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        player = other.transform;
        held = 0f;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        playerInRange = false;
        player = null;
        held = 0f;
        if (drone)
        {
            drone.SetProgress(0f);
            drone.ClearHint();
        }
    }
}
