using UnityEngine;

public class teleportAllowed : MonoBehaviour
{
    float nextAllowedTime = 0f;

    public bool CanTeleport()
    {
        return Time.time >= nextAllowedTime;
    }

    public void SetCooldown(float cooldown)
    {
        nextAllowedTime = Time.time + cooldown;
    }
}
