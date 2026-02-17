using bullet.fx.pack;
using System;
using UnityEngine;
using UnityEngine.Audio;

public class DroneBullet : MonoBehaviour
{

    [SerializeField] float life = 3f;
    [SerializeField] int damage = 2;
    [SerializeField] GameObject groundImpactVFX;
    [SerializeField] GameObject enemyImpactVFX;
    [SerializeField] DamageType damageType = DamageType.moving;
    [SerializeField] AudioMixerGroup sfxGroup;
    
    [SerializeField] AudioClip enemyHit;
    [SerializeField] AudioClip groundHit;
    [Range(0,1)][SerializeField] float hitVol;
    
    

    [SerializeField] LayerMask hitMask = ~0;
    Vector3 dir;
    float speed;
    
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Init(Vector3 d, float s)
    {
        dir = d.normalized;
        speed = s;
        Destroy(gameObject, life);
    }

    // Update is called once per frame
    void Update()
    {
        float step = speed * Time.deltaTime;

        if (Physics.Raycast(transform.position, dir, out var hit, step, hitMask, QueryTriggerInteraction.Ignore))
        {
            bool hitEnemy = false;

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg == null) 
                dmg = hit.collider.GetComponentInParent<IDamage>();

            if (dmg != null)

            {
                hitEnemy = true;

                dmg.takeDamage(damage, damageType);

            }
            if (hitEnemy)
            {   if (enemyImpactVFX)
                { 
                    Instantiate(enemyImpactVFX, hit.point, Quaternion.LookRotation(hit.normal)); 
                }

                PlayImpactSound(enemyHit);
            }
            else
            {
                if (groundImpactVFX)
                    Instantiate(groundImpactVFX, hit.point, Quaternion.LookRotation(hit.normal));

                PlayImpactSound(groundHit);
            }

            Destroy(gameObject);
            return;
        }
        transform.position += dir * step;
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
        a.volume = hitVol;
        a.minDistance = 3f;
        a.maxDistance = 40f;
        a.rolloffMode = AudioRolloffMode.Logarithmic;
        a.Play();

        Destroy(temp, clip.length);
    }
}
         

            
        

