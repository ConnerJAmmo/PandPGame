using UnityEngine;
using System.Collections;

public class damageDummy : MonoBehaviour, IDamage
{
    [SerializeField] Material material;
    [Range (1,10)][SerializeField] int HP;

    Color colorOrig;

    

   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = material.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator flashRed()
    {
        material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        material.color = colorOrig;
    }

    public void takeDamage(int amount)
    { 
        HP -= amount;

        if (HP <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(flashRed());
        }
    }
}
