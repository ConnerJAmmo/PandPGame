using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] string keyNeeded;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (gameManager.instance.playerScript.keyRing.Contains(keyNeeded))
            {
                Destroy(gameObject);
                gameManager.instance.playerScript.keyRing.Remove(keyNeeded);
            }
        }
    }
}
