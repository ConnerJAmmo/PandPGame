using UnityEngine;

[CreateAssetMenu]
public class GunStats : ScriptableObject
{
    public GameObject gunModel;

    [Range(3, 1200)] public int shootDist;
    [Range(1, 15)] public int shootDamage;
    [Range(0.1f, 4)] public float shootRate;

    public int ammoCur;
    [Range(5, 50)] public int ammoMax;

    public ParticleSystem hitEffect;
    public AudioClip[] shootSound;
    [Range(0, 1)] public float shootSoundVol;
}
