using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class damage : MonoBehaviour
{
    enum damageType { moving, stationary, DOT }

    [Header("Stats")]
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;

    [SerializeField] int damageAmount;
    [SerializeField] float damageRate;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;
    [SerializeField] GameObject hitEffect;

    //bool isDamaging;
    private Dictionary<IDamage, float> damageCooldowns = new Dictionary<IDamage, float>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(type == damageType.moving)
        {
            rb.linearVelocity = transform.forward * speed;
            Destroy(gameObject, destroyTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            return;
        }

        IDamage dmg = other.GetComponent<IDamage>();

        if(dmg != null && type != damageType.DOT)
        {
            dmg.takeDamage(damageAmount);
        }

        if(type == damageType.moving)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        /*
        if (other.isTrigger)
        {
            return;
        }
        */

        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg == null)
        {
            dmg = other.GetComponentInParent<IDamage>();
        }

        if (dmg != null && type == damageType.DOT)
        {
            //StartCoroutine(damageOther(dmg));
            if (!damageCooldowns.ContainsKey(dmg) || Time.time >= damageCooldowns[dmg])
            {
                dmg.takeDamage(damageAmount);
                damageCooldowns[dmg] = Time.time + damageRate;
            }
        }
    }
    /*
    IEnumerator damageOther (IDamage d)
    {
        isDamaging = true;
        d.takeDamage(damageAmount);
        yield return new WaitForSeconds(damageRate);
        isDamaging = false;
    }
    */

}
