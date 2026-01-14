using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class damage : MonoBehaviour
{
    enum damageType
    {
        moving,
        stationary,
        DOT
        //comment 
    }


    [Header("Type")]
    [SerializeField] damageType Type = damageType.moving;

    [Header("Projectile Movement")]
    [SerializeField] Rigidbody rb;
    [SerializeField] float lifeTime = 5f;
    [SerializeField] float speed; //change speed to a float for better accuracy

    [Header("DOT Settings (Only if we are using DOT)")]
    [SerializeField] float damageRate;

    [Header("Damage")]
    [SerializeField] int damageAmount;

    [Header("Hit Rules")]
    [SerializeField] bool destroyOnHit = true;
    [SerializeField] bool createHitEffect = true;
    [SerializeField] GameObject hitEffectPrefab;
    //[SerializeField] int destroyTime;

    [Header("Layer Rules")]
    [SerializeField] LayerMask damageLayers;
    [SerializeField] LayerMask blockLayers; // Walls, Terrain, Ground, 
 
    // This is just a variable to hold all of our DOT victims if we have more than one DOT zone
    private readonly HashSet<IDamage> dotVictims = new HashSet<IDamage>();

    //bool isDamaging;

    void Reset() // This is only used in the editor not during gameplay
                 // It is the same as hitting the 3 dots in the inspector and clicking reset
    {
        // We will use this to Auto-setup our component and make sure it work
        // This will find the Collider on the gameObject and force it to be a trigger
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;

        // This just turns gravity off on Rigidbodies and make sure physics is active
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;

            rb.isKinematic = (Type != damageType.moving);
        }

    }

    void Awake()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Type == damageType.moving)
        {
            if (rb != null)
            {
                rb.linearVelocity = transform.forward * speed;
            }

            if (lifeTime > 0f)
            {
                Destroy(gameObject, lifeTime);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) //not activated by another trigger
            return;

        int otherLayerMask = 1 << other.gameObject.layer;

        if ((blockLayers.value &  otherLayerMask) != 0)
        {
            DoHitFX(other.ClosestPoint(transform.position));

            if (Type == damageType.moving)
                Destroy(gameObject);

            return;
        }

        // If is not a damageable layer ignore it
        if ((damageLayers.value & otherLayerMask) == 0)
            return;


        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg == null)
        {
            dmg = other.GetComponentInParent<IDamage>();
        }

        if (dmg == null)
        {
            return;
        }

        if (Type == damageType.moving || Type == damageType.stationary)
        {
            dmg.takeDamage(damageAmount);

            DoHitFX(other.ClosestPoint(transform.position));

            if (Type == damageType.moving && destroyOnHit)
            {
                Destroy(gameObject);
            }
            
        }
        else if (Type == damageType.DOT)
        {
            // DOT type starts ticking while object inside
            if (!dotVictims.Contains(dmg))
            {
                dotVictims.Add(dmg);
                StartCoroutine(DotDamage(dmg));
            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (Type != damageType.DOT) return;

        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg == null) // && Type == damageType.DOT && !isDamaging
        {
            dmg = other.GetComponentInParent<IDamage>();
        }
        if (dmg != null)
            dotVictims.Remove(dmg);
    }

    IEnumerator DotDamage(IDamage target) // Timer function DOT type
    {
        while (dotVictims.Contains(target))
        {
            target.takeDamage(damageAmount);
            yield return new WaitForSeconds(damageRate);
        }

    }

    void DoHitFX(Vector3 point) // This is our function to spawn the effects where our arrow hit
    {
        if (!createHitEffect || hitEffectPrefab == null)
        {
            return;
        }

        Instantiate(hitEffectPrefab, point, Quaternion.identity);
    }


}
