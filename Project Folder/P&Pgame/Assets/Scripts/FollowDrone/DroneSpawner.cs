using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DroneSpawner : MonoBehaviour
{
    [Header("Spawn Area")]
    [SerializeField] Terrain terrain;
    [SerializeField] GameObject dronePrefab;

    [Header("Rules")]
    [SerializeField] float minDistanceFromPlayer = 120f;
    [SerializeField] float maxSlopeAngle = 25f;
    [SerializeField] int maxAttempts = 80;

    [Header("NavMesh Clamp")]
    [Tooltip("I put this here to keep the Drone clamped to the NavMesh")]
    [SerializeField] bool clampToNavMesh = true;
    [SerializeField] float navMeshSampleRadius = 5f;
    

    [Header("Building / Obstacle Avoidance")]
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] float clearanceRadius = 1.5f;
    [SerializeField] float spawnHeightOffset = 1.5f;



    void Start()
    {
        SpawnDrone();
    }

    void SpawnDrone()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (!terrain || !dronePrefab || !player)
            return;

        Vector3 playerPos = player.transform.position;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 point = RandomPointOnTerrain(terrain);

            // My distance rule
            if (Vector3.Distance(point, playerPos) < minDistanceFromPlayer)
                continue;

            // My slope rule, this make sure that the drone won't spawn on a high mountain/hill
            float slope = TerrainSlopeAngle(terrain, point);
            if (slope > maxSlopeAngle)
                continue;

            if (clampToNavMesh)
            {
                if (NavMesh.SamplePosition(point, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
                    point = hit.position;
                else
                    continue;
            }
            point.y += spawnHeightOffset;

            if (Physics.CheckSphere(point, clearanceRadius, obstacleMask, QueryTriggerInteraction.Ignore))
                continue;

            //if (requireReachablePath)
            //{
            //    NavMeshPath path = new NavMeshPath();
            //    bool haspath = NavMesh.CalculatePath(playerPos, point, NavMesh.AllAreas, path);
            //    if (!haspath || path.status != NavMeshPathStatus.PathComplete)
            //        continue;
            //}

            Instantiate(dronePrefab, point, Quaternion.identity);
            return;

        }
        // If we get pass our max attempts that means it could not find a spawn point
        Debug.LogWarning("DroneSpawner: Could not find a vaild Spawn Point");
    }

    private float TerrainSlopeAngle(Terrain ter, Vector3 worldPos)
    {
        Vector3 tp = ter.transform.position;
        Vector3 local = worldPos - tp;

        float clampX = Mathf.Clamp01(local.x / ter.terrainData.size.x);
        float clampZ = Mathf.Clamp01(local.z / ter.terrainData.size.z);

        Vector3 normal = ter.terrainData.GetInterpolatedNormal(clampX, clampZ);
        return Vector3.Angle(normal, Vector3.up);
    }

    private Vector3 RandomPointOnTerrain(Terrain ter)
    {
        Vector3 tp = ter.transform.position;
        Vector3 size = ter.terrainData.size; //terrainData stores hieght maps, trees, terrain textures

        float x = tp.x + Random.Range(0f, size.x);
        float z = tp.z + Random.Range(0f, size.z);
        float y = ter.SampleHeight(new Vector3(x, 0f, z)) + tp.y;

        return new Vector3(x, y, z);
    }

    
}
