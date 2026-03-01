using UnityEngine;

[CreateAssetMenu]
public class GunStats : ScriptableObject
{
    public GameObject gunModel;


    public string gunName;
    public int DistLevel;
    [Range(3, 1200)] public int shootDist;
    public int rangeUpgradeCost;
    public int shootDistOrig;
    public int damageLevel;
    [Range(1, 50)] public int shootDamage;
    public GameObject bulletPrefab;
    public int damageUpgradeCost;
    public int shootDamageOrig;
    public int maxAmmoLevel;
    [Range(0.1f, 4)] public float shootRate;
    public int maxAmmoOrig;
    public int maxAmmoUpgradeCost;


    public int ammoCur;
    [Range(5, 50)] public int ammoMax;

    public Transform shootPos;

    public ParticleSystem hitEffect;
    public ParticleSystem muzzleFlashEffect;
    public AudioClip[] shootSound;
    [Range(0, 1)] public float shootSoundVol;
}
