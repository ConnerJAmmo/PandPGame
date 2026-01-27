using UnityEngine;

public class pickupGuns : MonoBehaviour
{

    [SerializeField] GunStats gun;

    private void OnTriggerEnter(Collider other)
    {
        IPickup pick = other.GetComponent<IPickup>();

        if(pick != null)
        {
            gun.ammoCur = gun.ammoMax;
            pick.getGunStats(gun);
            Destroy(gameObject);
        }
    }


}
