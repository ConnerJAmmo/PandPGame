using UnityEngine;

public class CrystalPickup : MonoBehaviour
{
    [SerializeField] int amount = 1;
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip pickupSfx;
    [Range(0, 1)] [SerializeField] float crystalAudVol = 0.5f;
   
    

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        var inv = other.GetComponent<PlayerCrystalInventory>();
        if (!inv)
            inv = other.gameObject.AddComponent<PlayerCrystalInventory>();

        inv.AddCrystals(amount);

        if (aud && pickupSfx)
            aud.PlayOneShot(pickupSfx, crystalAudVol);
        Destroy(gameObject);


    }
}
