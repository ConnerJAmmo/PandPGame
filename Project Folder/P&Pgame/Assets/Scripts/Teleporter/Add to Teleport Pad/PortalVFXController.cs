using UnityEngine;
using UnityEngine.VFX;

public class PortalVFXController : MonoBehaviour
{
    [SerializeField] VisualEffect vfx;

    private void Awake()
    {
        if (!vfx) vfx = GetComponentInChildren<VisualEffect>();
    }

    public void PlayEnter()
    {
        if (vfx)
            vfx.SendEvent("OnEnter");
        Debug.Log("Enter Event Sent");
    }

    public void PlayExit()
    {
        if (vfx)
            vfx.SendEvent("OnExit");
    }
}
