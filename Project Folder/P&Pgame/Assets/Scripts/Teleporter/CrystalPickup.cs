using UnityEngine;
using UnityEngine.Audio;

public class CrystalPickup : MonoBehaviour
{
    [SerializeField] int amount = 1;
    [SerializeField] AudioSource aud;
    [SerializeField] AudioMixerGroup sfxGroup;
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

        
        PlayImpactSound(pickupSfx);

        Destroy(gameObject);
    }

    private void PlayImpactSound(AudioClip clip)
    {
        if (!clip) return;

        GameObject temp = new GameObject("DroneImpactSound");
        temp.transform.position = transform.position;

        AudioSource a = temp.AddComponent<AudioSource>();

        a.outputAudioMixerGroup = sfxGroup;
        a.clip = clip;
        a.spatialBlend = 1f;
        a.volume = crystalAudVol;
        a.minDistance = 3f;
        a.maxDistance = 40f;
        a.rolloffMode = AudioRolloffMode.Logarithmic;
        a.Play();

        Destroy(temp, clip.length);
    }
}
