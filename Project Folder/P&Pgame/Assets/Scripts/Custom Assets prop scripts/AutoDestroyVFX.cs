using UnityEngine;

public class AutoDestroyVFX : MonoBehaviour
{
    [SerializeField] float life = 2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, life);
    }

    
}
