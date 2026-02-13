using bullet.fx.pack;
using System.Collections;
using UnityEngine;

public class ForceField : MonoBehaviour, IDamage
{
    [Header("Stats")]
    [Range(1, 1000)][SerializeField] public int maxHP;

    [Header("---------Audio--------")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] takeDamageAud;
    [SerializeField] float takeDamageVol;
    [SerializeField] AudioClip[] destroyedAud;
    [SerializeField] float destroyedVol;

    public int HP;

    private Color colorOrigin;
    private Material dynamicMat;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Use .material to create a local instance for this object
        dynamicMat = GetComponentInChildren<Renderer>().material;

        // Use the specific reference name instead of .color
        colorOrigin = dynamicMat.color;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void IDamage.takeDamage(int amount, DamageType type)
    {
        HP -= amount;

        // Debug to prove it's this specific instance
        Debug.Log($"{gameObject.name} took {amount} damage. HP left: {HP}");

        if (HP <= 0)
        {
            this.gameObject.GetComponentInParent<TurretPlacement>().hasShield = false;
            Destroy(gameObject);
        }
        else
        {
            // Stop only the flash coroutine to prevent color getting stuck
            StopCoroutine(flashRed());
            StartCoroutine(flashRed());

            if (HP <= 20)
            {
                aud.PlayOneShot(destroyedAud[0], destroyedVol);
            }
            else
                aud.PlayOneShot(takeDamageAud[Random.Range(0, takeDamageAud.Length)], takeDamageVol);
        }
    }

    IEnumerator flashRed()
    {
        dynamicMat.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        dynamicMat.color = colorOrigin;
    }
}
