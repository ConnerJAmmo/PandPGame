using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PortalRTWebGLFix : MonoBehaviour
{
    [Header("Assignments")]
    [SerializeField] Camera portalCam;
    [SerializeField] RawImage debugRawImage;
    [SerializeField] MeshRenderer portalSurface;
    [SerializeField] string portalTextProp = "_PortalTex";

    [Header("RT settings")]
    [SerializeField] int size = 512;
    [SerializeField] bool assignToMaterial = true;
    [SerializeField] bool assignToRawImage = true;

    RenderTexture rt;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            enabled = false;
            return;
        }
        if(!portalCam) portalCam = GetComponent<Camera>();
    }

    // Update is called once per frame
    IEnumerator Start()
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer)
            yield break;

        // wait 1 frame so WebGL/URP has initialized
        yield return null;

        CreateRT();
        AssignRT();

        portalCam.enabled = true;
        portalCam.Render();

        yield return null;
        AssignRT();
    }

    private void AssignRT()
    {
        if (!portalCam || rt == null) return;

        portalCam.targetTexture = rt;

        if (assignToRawImage && debugRawImage)
            debugRawImage.texture = rt;

        if (assignToMaterial && portalSurface &&  portalSurface.sharedMaterial)
        {
            portalSurface.sharedMaterial.SetTexture(portalTextProp, rt);
        }
    }

    private void CreateRT()
    {
        if (rt != null)
        {
            portalCam.targetTexture = null;
            rt.Release();
            Destroy(rt);
        }

        var desc = new RenderTextureDescriptor(size, size)
        {
            graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm,
            depthBufferBits = 16,
            msaaSamples = 1,
            sRGB = (QualitySettings.activeColorSpace == ColorSpace.Linear),
            useMipMap = false,
            autoGenerateMips = false
        };

        rt = new RenderTexture(desc)
        {
            name = "RT_Portal_RunTime",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        rt.Create();
    }

    private void OnDestroy()
    {
        if (rt != null)
        {
            rt.Release(); 
            Destroy(rt);
        }
    }
}
