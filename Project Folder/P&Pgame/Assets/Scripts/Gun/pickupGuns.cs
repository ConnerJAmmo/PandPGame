using UnityEngine;

public class pickupGuns : MonoBehaviour
{

    [SerializeField] GunStats gun;
    [SerializeField] Transform shootPos;

    private void OnTriggerEnter(Collider other)
    {
        IPickup pick = other.GetComponent<IPickup>();

        if(pick != null)
        {
            gun.ammoCur = gun.ammoMax;
            gameManager.instance.playerScript.shootPos = shootPos;
            gun.shootPos = shootPos;
            pick.getGunStats(gun);
            Destroy(gameObject);
            gameManager.instance.UpdateAmmoUI(gun.ammoCur, gun.ammoMax);
            gameManager.instance.playerScript.resetGunStatsToOrig();
        }
    }


}
