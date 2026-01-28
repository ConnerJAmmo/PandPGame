using UnityEngine;

[CreateAssetMenu]
public class GunStats : ScriptableObject
{
    public GameObject gunModel;

    public string gunName;
    [Range(3, 1200)] public int shootDist;
    public int shootDistOrig;
    [Range(1, 15)] public int shootDamage;
    public int shootDamageOrig;
    [Range(0.1f, 4)] public float shootRate;
    public float shootRateOrig;

    public int ammoCur;
    [Range(5, 50)] public int ammoMax;

    public ParticleSystem hitEffect;
    public AudioClip[] shootSound;
    [Range(0, 1)] public float shootSoundVol;
}
