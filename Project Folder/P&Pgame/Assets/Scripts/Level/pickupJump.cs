using Unity.VisualScripting;
using UnityEngine;

public class pickupJump : MonoBehaviour
{
    [Header("Jump Boost Settings")]
    [SerializeField][Range(1,2)] private int jumpIncrease;


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
                ApplyJumpBoost(other.gameObject);
                audioSource.PlayOneShot(pickupSound, pickupVolume);

                Destroy(gameObject);
            }
        }
    }

    private void ApplyJumpBoost(GameObject player)
    {
        PlayerCont playerController = player.GetComponent<PlayerCont>();

        if (playerController != null)
        {
            GameData.instance.PlayerJumpBoost += jumpIncrease;
            playerController.ApplyJumpBoost(jumpIncrease);
            gameManager.instance.ShowNotification($"Propulsion boots found! +1 to max jumps!");
        }
    }
}
