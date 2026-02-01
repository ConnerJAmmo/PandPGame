using bullet.fx.pack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class baseDmg : MonoBehaviour, IDamage
{
    [Header("Stats")]
    [SerializeField] Renderer model;
    [Range(1, 1000)][SerializeField] int maxHP = 1000;
    int hp;

    [Header("Tower Health Bar")]
    [SerializeField] TowerHealth bar;

    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] baseTakeDamageAud;
    [Range(0, 1)] [SerializeField] float baseTakeDamageVol;
    [SerializeField] AudioClip[] baseDestroyedAud;
    [Range(0, 1)] [SerializeField] float baseDestroyedVol;


    Color colorOrigin; 
    Material dynamicMat; // Store the unique instance material



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hp = maxHP;

        dynamicMat = model.material;
        colorOrigin = dynamicMat.color;

        if (!bar) bar = GetComponent<TowerHealth>();
        if (bar) bar.updateBar(1f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void takeDamage(int amount, DamageType type)
    {
        hp -= amount;
        if (hp < 0)
            hp = 0;

        if (bar) bar.updateBar((float)hp / maxHP);
        

        // Debugging to verify the instance is taking damage
        Debug.Log($"{gameObject.name} (Base) took damage! Remaining HP: {hp}");
        Debug.Log("Basedmg hit by: " + type);
        if (hp <= 0)
        {
            gameManager.instance.youLose();
            
            Destroy(gameObject);
        }
        else
        {
            // Stop the specific flash routine so colors don't get stuck
            StopCoroutine("flashRed");
            StartCoroutine(flashRed());
            if (hp <= 20)
            {
                aud.PlayOneShot(baseDestroyedAud[0], baseDestroyedVol);
            }else
                aud.PlayOneShot(baseTakeDamageAud[Random.Range(0, baseTakeDamageAud.Length)], baseTakeDamageVol);
            
        }
    }

    IEnumerator flashRed()
    {
        // Use the dynamicMat reference
        dynamicMat.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        dynamicMat.color = colorOrigin;
    }
}
