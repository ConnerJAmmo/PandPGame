using UnityEngine;

public class PortalLink : MonoBehaviour
{
    [Header("Link")]
    [SerializeField] string portalId = "PadA";
    [SerializeField] string destinationPortalId = "PadB";

    [Header("Cam Anchors")]
    [Tooltip("I put these here for the RanderCam Anchor Position")]
    private Camera portalCam;
    [SerializeField] Transform thisAnchor; // for this pad
    private Transform destinationAnchor; // For Dest pad

    [Header("Performance")]
    [SerializeField] float enableDistance = 18f;

    [SerializeField] PlayerTeleporter teleporter;
    
    Transform player;
    PortalLink destination;


    private void Reset()
    {
        AutoWire();
    }
    private void Awake()
    {
        AutoWire();
        FindPlayer();
        CacheDestination();
    }

    void AutoWire()
    {
        if (!portalCam) portalCam = GetComponentInChildren<Camera>(true);
        if (!thisAnchor)
        {
            thisAnchor = FindChildByName(transform, "PortalCamAnchor");

            if (!thisAnchor)
                thisAnchor = FindChildStartsWith(transform, "PortalCamAnchor");
        }

        if (!teleporter) teleporter = GetComponentInParent<PlayerTeleporter>(true);
    }

    void FindPlayer()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go)
        {
            player = go.transform;
        }

    }

    void CacheDestination()
    {
        destination = null;
        destinationAnchor = null;

        var all = Object.FindObjectsByType<PortalLink>(FindObjectsSortMode.None);
        foreach (var p in all)
        {
            if (p != this && p.portalId == destinationPortalId)
                    {
                destination = p;
                break;
            }
        }

        if (!destination) return;

        if (destination.thisAnchor)
            destinationAnchor = destination.thisAnchor;
        else
        {
            destinationAnchor = FindChildByName(destination.transform, "PortalCamAnchor");
            if (!destinationAnchor) destinationAnchor = FindChildStartsWith(destination.transform, "PortalCamAnchor");
        }
    }

    private void LateUpdate()
    {
        if (teleporter && !teleporter.IsActive)
        {
            if (portalCam && portalCam.enabled)
                portalCam.enabled = false;
            return;
        }
        if (!player)
        {
            FindPlayer();
            if (!player)
                return;
        }
        if (!destination)
        {
            CacheDestination();
            if (!destination)
                return;
        }

        if (!portalCam)
            return;

        float dist = Vector3.Distance(player.position, transform.position);
        bool shouldRender = dist <= enableDistance;

        if (portalCam.enabled != shouldRender)
        {
            portalCam.enabled = shouldRender;
        }

        if (!shouldRender) return;

        Transform from, to;

        // this Anchor check
        if (thisAnchor)
            from = thisAnchor;
        else
            from = transform;

        // destination anchor check
        if (destinationAnchor)
            to = destinationAnchor;
        else
            to = destination.transform;

        // position
        Vector3 localPos = from.InverseTransformPoint(player.position); // world space to local space
        localPos = new Vector3(-localPos.x, localPos.y, - localPos.z); // mirror
        portalCam.transform.position = to.TransformPoint(localPos);

        // rotation
        Quaternion localRot = Quaternion.Inverse(from.rotation) * player.rotation;
        portalCam.transform.rotation = to.rotation * localRot;

    }

    static Transform FindChildByName(Transform root, string exactName)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true)) 
            if (t.name == exactName) return t;
        return null;
    }

    static Transform FindChildStartsWith(Transform root, string startsWith)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            if (t.name.StartsWith(startsWith)) return t;
        return null;
    }
}
