using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class baseDmg : MonoBehaviour, IDamage
{
    [Header("Stats")]
    [SerializeField] Renderer model;
    [Range(1, 1000)][SerializeField] int HP;

    Color colorOrigin;
    float nextDamageTime;

    [SerializeField] List<Collider> enemiesInRange = new List<Collider>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrigin = model.material.color;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInRange.Add(other);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInRange.Remove(other);
        }
    }

    // Update is called once per frame
    void Update()
    {
        enemiesInRange.RemoveAll(enemy => enemy == null);

        if (enemiesInRange.Count > 0 && Time.time >= nextDamageTime)
        {
            // Damage = 1 per enemy in the list
            takeDamage(enemiesInRange.Count);
            nextDamageTime = Time.time + 1f;
        }
    }

    public void takeDamage(int amount)
    {
        HP -= Mathf.Min(amount, 5);
        if (HP <= 0)
        {
            gameManager.instance.youLose();
            Destroy(gameObject);
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(flashRed());
        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrigin;
    }

}
