using UnityEngine;

public class CleanupParent : MonoBehaviour
{
    void Start()
    {
        // 1. Tell all children to become independent in the world
        // DetachChildren() is more efficient than a loop for this
        transform.DetachChildren();

        // 2. Delete this empty container immediately
        Destroy(gameObject);
    }
}
