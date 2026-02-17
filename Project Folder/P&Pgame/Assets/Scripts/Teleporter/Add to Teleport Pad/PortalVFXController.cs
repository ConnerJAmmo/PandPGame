using UnityEngine;
using UnityEngine.VFX;

public class PortalVFXController : MonoBehaviour
{
    [SerializeField] VisualEffect vfx;
    [SerializeField] GameObject vfxRoot;

    private void Awake()
    {
        if (!vfx) vfx = GetComponentInChildren<VisualEffect>();
        if (!vfxRoot && vfx) vfxRoot = vfx.gameObject;
    }

    public void PlayOn()
    {
        if (vfxRoot)
            vfxRoot.SetActive(true);
        if (vfx)
            vfx.Play();  //start looping system

    }

    public void PlayOff()
    {
        if (vfx)
            vfx.Stop();
        if(vfxRoot)
            vfxRoot.SetActive(false);

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
