using UnityEngine;

public class pickupKeys : MonoBehaviour
{

    [SerializeField] string keyName;

    private void OnTriggerEnter(Collider other)
    {
        IPickupKeys pick = other.GetComponent<IPickupKeys>();

        if(pick != null)
        {
            pick.getKey(keyName);
            Destroy(gameObject);
            
        }
    }


}
