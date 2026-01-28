using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkinnedCollisionUpdater : MonoBehaviour
{
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private MeshCollider meshCollider;
    private Mesh bakedMesh;

    [Header("Settings")]
    [SerializeField] private float updatesPerSecond = 5f;
    [SerializeField] private float detectionRadius = 50f; // Only bake if target is closer than this
    [SerializeField] private LayerMask detectionLayers;
    void Start()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        bakedMesh = new Mesh();
        StartCoroutine(UpdateColliderRoutine());
    }

    IEnumerator UpdateColliderRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(1f / updatesPerSecond);

        while (true)
        {
            bool targetNearby = Physics.CheckSphere(transform.position, detectionRadius, detectionLayers);

            if (targetNearby)
            {
                UpdateCollider();
            }
            else if (meshCollider.enabled)
            {
                meshCollider.enabled = false;
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

    // Visualizes the detection range in the Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
