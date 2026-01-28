using UnityEngine;

public class pickupGuns : MonoBehaviour
{

    [SerializeField] GunStats gun;

    private void Start()
    {
        gun.shootDamageOrig = gun.shootDamage;
        gun.shootDistOrig = gun.shootDist;
        gun.shootRateOrig = gun.shootRate;
    }

    private void OnTriggerEnter(Collider other)
    {
        IPickup pick = other.GetComponent<IPickup>();

        if(pick != null)
        {
            gun.ammoCur = gun.ammoMax;
            pick.getGunStats(gun);
            Destroy(gameObject);
            gameManager.instance.UpdateAmmoUI(gun.ammoCur, gun.ammoMax);
        }
    }


}
