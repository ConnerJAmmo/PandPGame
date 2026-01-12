using UnityEngine;
using System.Collections;

public class Damage : MonoBehaviour
{
    enum damageType
    {
        moving,
        stationary,
        DOT,
        homing
    }

    [SerializeField] damageType Type;
    [SerializeField] Rigidbody rb;

    [SerializeField] int damageAmount;
    [SerializeField] float damageRate;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;
    [SerializeField] GameObject hitEffect;

    bool isDamaging;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Type == damageType.moving)
        {
            rb.linearVelocity = transform.forward * speed;
            Destroy(gameObject, destroyTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) //not activated by another trigger
            return;

        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg != null && Type != damageType.DOT)
        {
            dmg.takeDamage(damageAmount);
        }

        if(Type == damageType.moving) 
        { 
            Destroy(gameObject); 
        }

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger) return;

        IDamage dmg = other.GetComponent <IDamage>();
        if (dmg != null && Type == damageType.DOT && !isDamaging)
        {
            StartCoroutine(damageOther(dmg));
        }
    }

    IEnumerator damageOther(IDamage d)
    {
        isDamaging = true;
        d.takeDamage(damageAmount);
        yield return new WaitForSeconds(damageRate);
        isDamaging = false;
    }

}
