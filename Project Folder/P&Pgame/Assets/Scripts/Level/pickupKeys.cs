using UnityEngine;

public class pickupKeys : MonoBehaviour
{

    [SerializeField] string keyName;

    private void OnTriggerEnter(Collider other)
    {
        IPickupKeys pick = other.GetComponent<IPickupKeys>();

        if (other.CompareTag("Player"))
{
            if (pick != null)
            {
                pick.getKey(keyName);
                Destroy(gameObject);

            }
        }
    }


}
