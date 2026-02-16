using UnityEngine;

public class powerCrystal : MonoBehaviour
{
    //Yes, the power emerald... I can feel the POWER!
    [Header("Numbers")]
    [SerializeField] int amount;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField][Range(0, 1)] private float pickupVolume = 0.5f;


    private void OnTriggerEnter(Collider other)
    {
        
    
        IPickupGeneric pickerUpper = other.GetComponent<IPickupGeneric>();

        if (pickerUpper != null)
        {

            pickerUpper.getGeneric("Power Crystal", amount);

            audioSource.PlayOneShot(pickupSound);
            gameManager.instance.ShowNotification("Power Crystal found! Bring it to the Spire to swiftly eradicate foes!");

            Destroy(gameObject);

        }
    }
}
