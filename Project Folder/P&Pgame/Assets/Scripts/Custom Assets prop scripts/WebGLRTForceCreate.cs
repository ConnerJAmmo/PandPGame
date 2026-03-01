using UnityEngine;

public class WebGLRTForceCreate : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       var cam = GetComponent<Camera>();
        
       if (cam.targetTexture != null )
        {
            cam.targetTexture.Release();
            cam.targetTexture.Create();
        }
       cam.enabled = true;
    }
}
