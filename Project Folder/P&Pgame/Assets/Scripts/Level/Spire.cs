using UnityEngine;

public class Spire : MonoBehaviour
{
    [SerializeField] GameObject AOE;

    [SerializeField] float boomTime;

    bool kaboom;

    float boomTimer;

    private void Start()
    {

        kaboom = false;

        boomTimer = 0;
    }

    private void Update()
    {


        if (kaboom)
        {
            boomTimer += Time.deltaTime;

            if (boomTimer >= boomTime)
            {
                AOE.SetActive(false);

                AOE.GetComponent<SphereCollider>().enabled = false;
            }
            
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (gameManager.instance.playerScript.powerCrystals >= 1)
            {
                gameManager.instance.playerScript.powerCrystals = gameManager.instance.playerScript.powerCrystals - 1;

                AOE.SetActive(true);

                AOE.GetComponent<SphereCollider>().enabled = true;

                kaboom = true;
            }
        }
    }

    
}
