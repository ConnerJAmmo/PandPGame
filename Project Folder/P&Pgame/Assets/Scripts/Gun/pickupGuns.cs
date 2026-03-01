using bullet.fx.pack;
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
            if (gun.gunName == "Machine Gun")
            {
                gun.shootPos = gameManager.instance.playerScript.machineGunShootPos;
            }
            else if (gun.gunName == "M1918 Bar")
            {
                gun.shootPos = gameManager.instance.playerScript.m1918BarShootPos;
            }
            else
            {
                gun.shootPos = gameManager.instance.playerScript.m1GarandShootPos;
            }
            pick.getGunStats(gun);
            Destroy(gameObject);
            gameManager.instance.UpdateAmmoUI(gun.ammoCur, gun.ammoMax);
            gameManager.instance.playerScript.resetGunStatsToOrig();
        }
    }


}
