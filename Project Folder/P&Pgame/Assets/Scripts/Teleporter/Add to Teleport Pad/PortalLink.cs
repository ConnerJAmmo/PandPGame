using UnityEngine;

public class PortalLink : MonoBehaviour
{
    [Header("Link")]
    [SerializeField] Transform destinationPortal;
    [SerializeField] Camera portalCam;

    [Header("Cam Anchors")]
    [Tooltip("I put these here for the RanderCam Anchor Position")]
    [SerializeField] Transform thisAnchor; // for this pad
    [SerializeField] Transform destinationAnchor; // For Dest pad

    [Header("Player")]
    [SerializeField] Transform player;

    [Header("Performance")]
    [SerializeField] float enableDistance;

    [SerializeField] PlayerTeleporter teleporter;

    private void LateUpdate()
    {
        if (teleporter && !teleporter.IsActive) return;

        if (!player || !destinationPortal || !portalCam)
            { return; }

        float dist = Vector3.Distance(player.position, transform.position);
        bool shouldRender = dist < enableDistance;

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
            to = destinationPortal;

        // position
        Vector3 localPos = from.InverseTransformPoint(player.position); // world space to local space
        localPos = new Vector3(-localPos.x, localPos.y, - localPos.z); // mirror
        portalCam.transform.position = to.TransformPoint(localPos);

        // rotation
        Quaternion localRot = Quaternion.Inverse(from.rotation) * player.rotation;
        portalCam.transform.rotation = to.rotation * localRot;

    }

}
