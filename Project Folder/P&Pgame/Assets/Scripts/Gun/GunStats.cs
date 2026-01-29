using UnityEngine;

[CreateAssetMenu]
public class GunStats : ScriptableObject
{
    public GameObject gunModel;


    public string gunName;
    public int DistLevel;
    [Range(3, 1200)] public int shootDist;
    public int shootDistOrig;
    public int damageLevel;
    [Range(1, 15)] public int shootDamage;
    public int shootDamageOrig;
    public int fireRateLevel;
    [Range(0.1f, 4)] public float shootRate;
    public float shootRateOrig;

    public int ammoCur;
    [Range(5, 50)] public int ammoMax;

    public ParticleSystem hitEffect;
    public AudioClip[] shootSound;
    [Range(0, 1)] public float shootSoundVol;
}
