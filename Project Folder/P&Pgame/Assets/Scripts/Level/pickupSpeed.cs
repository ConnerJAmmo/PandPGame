using Unity.VisualScripting;
using UnityEngine;

public class pickupSpeed : MonoBehaviour
{
    [Header("Speed Boost Settings")]
    [SerializeField][Range(1,2)] private int speedIncrease;
    [SerializeField] private GameObject pickupEffect;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField][Range(0,1)] private float pickupVolume = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IPickup pickup = other.GetComponent<IPickup>();

            if (pickup != null)
            {
                ApplySpeedBoost(other.gameObject);
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
                audioSource.PlayOneShot(pickupSound, pickupVolume);

                Destroy(gameObject);
            }
        }
    }

    private void ApplySpeedBoost(GameObject player)
    {
        PlayerCont playerController = player.GetComponent<PlayerCont>();

        if (playerController != null)
        {
            GameData.instance.PlayerSpeedBoost += speedIncrease;
            playerController.ApplySpeedBoost(speedIncrease);
            gameManager.instance.ShowNotification($"Feather of Speed found! \n+{speedIncrease} to speed!");
        }
    }
}
