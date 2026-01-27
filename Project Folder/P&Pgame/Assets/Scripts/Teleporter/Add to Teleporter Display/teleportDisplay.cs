using UnityEngine;

public class teleportDisplay : MonoBehaviour
{
    [Header("Change Pad Number for each Pad (Don't Forget)")]
    [SerializeField] string message = "Teleport available: Going To Pad 2";

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log($"{name} ENTER by {other.name} tag = {other.tag}");
        if (!other.CompareTag("Player")) return;
        gameManager.instance.SetInteractionHint(message);
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.Log($"{name} EXIT by {other.name} tag = {other.tag}");
        if (!other.CompareTag("Player")) return;
        gameManager.instance.ClearInteractionHint();
    }
}
