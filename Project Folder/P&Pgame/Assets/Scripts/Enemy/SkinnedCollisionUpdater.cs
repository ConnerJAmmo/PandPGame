using UnityEngine;
using System.Collections;

public class SkinnedCollisionUpdater : MonoBehaviour
{
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private MeshCollider meshCollider;
    private Mesh bakedMesh;
    private Transform playerTransform;

    [Header("Settings")]
    [SerializeField] private float updatesPerSecond = 5f;
    [SerializeField] private float detectionRadius = 50f; // Only bake if player is closer than this

    void Start()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        bakedMesh = new Mesh();

        // Find the player transform using the "Player" tag
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;

        StartCoroutine(UpdateColliderRoutine());
    }

    IEnumerator UpdateColliderRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(1f / updatesPerSecond);

        while (true)
        {
            if (playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);

                if (distance <= detectionRadius)
                {
                    UpdateCollider();
                }
                else
                {
                    // Optional: Disable collider when too far to save even more CPU
                    if (meshCollider.enabled) meshCollider.enabled = false;
                }
            }
            yield return wait;
        }
    }

    private void UpdateCollider()
    {
        if (!meshCollider.enabled) meshCollider.enabled = true;

        skinnedMeshRenderer.BakeMesh(bakedMesh, true);
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = bakedMesh;
    }
}
