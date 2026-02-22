using System.Collections;
using UnityEngine;

public class SpireAOE : MonoBehaviour
{
    [SerializeField] float boomsPerSecond;
    [SerializeField] int damage;

    float boomTimer;

    bool isDamaging;
    

    private void Start()
    {
        boomTimer = 0;

        isDamaging = false;
    }

    private void Update()
    {
        boomTimer += Time.deltaTime;
    }


    private void OnTriggerStay(Collider other)
    {


        if (other.CompareTag("Enemy"))
        {
            //Debug.Log("Active");

            IDamage ouchie = other.GetComponent<IDamage>();

            if (boomTimer >= boomsPerSecond)
            {
                ouchie.takeDamage(damage, bullet.fx.pack.DamageType.stationary);
            }
        }
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    if (other.isTrigger)
    //        return;

    //    if (other.CompareTag("Player"))
    //        return;

    //    IDamage dmg = other.GetComponent<IDamage>();
    //    if (dmg != null)
    //    {
    //        StartCoroutine(damageOther(dmg));
    //    }
    //}

    //IEnumerator damageOther(IDamage d)
    //{
    //    isDamaging = true;
    //    d.takeDamage(damage, bullet.fx.pack.DamageType.stationary);
    //    yield return new WaitForSeconds(boomsPerSecond);
    //    isDamaging = false;
    //}


}
