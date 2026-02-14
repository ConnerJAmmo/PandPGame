using UnityEngine;
using System.Collections;

public class materialFunctions : MonoBehaviour, IMaterial
{
    enum materialSelect { Wood, Stone, Metal }

    [SerializeField] materialSelect type;

    [SerializeField] Material materialMaterial;
    

    [SerializeField] int totalMaterial;

    Color colorOrig;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = materialMaterial.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int materialDamage(int amount)
    {
        if (totalMaterial <= amount)
        {
            amount = 0 + totalMaterial;
            Destroy(gameObject);
        }
        else
        {
            totalMaterial = totalMaterial - amount;
            StartCoroutine(flashRed());
        }

        return amount;
    }

    public string materialType()
    {
        string typer = "";
        if (type == materialSelect.Wood)
        {
            typer = "Wood";
        }
        else if (type == materialSelect.Stone)
        {
            typer = "Stone";
        }
        else if (type == materialSelect.Metal)
        {
            typer = "Metal";
        }

        return typer;
    }

    IEnumerator flashRed()
    {
        materialMaterial.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        materialMaterial.color = colorOrig;
    }
}