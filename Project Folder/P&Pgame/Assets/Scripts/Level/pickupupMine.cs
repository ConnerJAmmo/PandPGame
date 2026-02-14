using Unity.VisualScripting;
using UnityEngine;

public class pickupMine : MonoBehaviour
{
    [Header("Speed Boost Settings")]
    [SerializeField][Range(1,2)] private int miningSpeedIncrease;


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
                ApplyMiningSpeedBoost(other.gameObject);
                audioSource.PlayOneShot(pickupSound, pickupVolume);

                Destroy(gameObject);
            }
        }
    }

    private void ApplyMiningSpeedBoost(GameObject player)
    {
        PlayerCont playerController = player.GetComponent<PlayerCont>();

        if (playerController != null)
        {
            GameData.instance.PlayerMiningSpeedBoost += miningSpeedIncrease;
            playerController.ApplyMiningSpeedBoost(miningSpeedIncrease);
            gameManager.instance.ShowNotification($"Rocket Powered Pickaxe found! \nIncrease to  mining speed!");
        }
    }
}