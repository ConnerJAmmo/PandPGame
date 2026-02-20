using Unity.VisualScripting;
using UnityEngine;

public class ShipDoorTrigger : MonoBehaviour
{
    [SerializeField] ShipDoorController door;

    [SerializeField] bool consumeKey = true;
    [SerializeField] Collider doorBlockerCollider;

    [SerializeField] string requiredKeyName = "ShipKey";
    [SerializeField] string openMsg = "Door Opening... ";

    bool isOpen;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isOpen) return;
        

        var playerScript = gameManager.instance.playerScript;

        var keyRing = playerScript.keyRing;

        if (keyRing != null && keyRing.Contains(requiredKeyName))

        {
            if (consumeKey)
                keyRing.Remove(requiredKeyName);
           
            door.Open();
            isOpen = true;
            
            gameManager.instance.SetInteractionHint(openMsg);
            
            if (doorBlockerCollider)
                doorBlockerCollider.enabled = false;
    
            gameManager.instance.ClearInteractionHint();
        }else
        {
            gameManager.instance.SetInteractionHint($"Door Locked. Need: {requiredKeyName}");
        }



        
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if(!isOpen)
            gameManager.instance.ClearInteractionHint();
    }

}
