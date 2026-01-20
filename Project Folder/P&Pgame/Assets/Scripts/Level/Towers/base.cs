using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class baseDmg : MonoBehaviour, IDamage
{
    [Header("Stats")]
    [SerializeField] Renderer model;
    [Range(1, 1000)][SerializeField] int HP = 1000;

    Color colorOrigin; 
    Material dynamicMat; // Store the unique instance material


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dynamicMat = model.material;
        colorOrigin = dynamicMat.color;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        // Debugging to verify the instance is taking damage
        Debug.Log($"{gameObject.name} (Base) took damage! Remaining HP: {HP}");

        if (HP <= 0)
        {
            gameManager.instance.youLose();
            Destroy(gameObject);
        }
        else
        {
            // Stop the specific flash routine so colors don't get stuck
            StopCoroutine("flashRed");
            StartCoroutine(flashRed());
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
