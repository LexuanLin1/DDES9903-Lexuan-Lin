using UnityEngine;

public sealed class InfiniteCorridorTeleporter : MonoBehaviour
{
    [Header("Security")]
    [SerializeField]
    private MissionSecurityController securityController;

    [Header("Teleport")]
    [SerializeField]
    private Transform returnPoint;

    [SerializeField, Min(0.1f)]
    private float teleportCooldown = 0.75f;

    [Header("Post-Teleport Narrative")]
    [SerializeField]
    private PostTeleportNarrativeSequence postTeleportNarrative;

    private float nextAllowedTeleportTime;

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time < nextAllowedTeleportTime ||
            securityController == null ||
            !securityController.SecurityActive ||
            returnPoint == null)
        {
            return;
        }

        CharacterController player =
            other.GetComponentInParent<CharacterController>();

        if (player == null ||
            !player.gameObject.activeInHierarchy)
        {
            return;
        }

        nextAllowedTeleportTime =
            Time.time + teleportCooldown;

        Transform playerTransform = player.transform;
        Quaternion currentRotation = playerTransform.rotation;
        bool controllerWasEnabled = player.enabled;

        if (controllerWasEnabled)
        {
            player.enabled = false;
        }

        playerTransform.SetPositionAndRotation(
            returnPoint.position,
            currentRotation
        );

        Physics.SyncTransforms();

        if (controllerWasEnabled &&
            player.gameObject.activeInHierarchy)
        {
            player.enabled = true;
        }

        securityController.RegisterCorridorLoop();

        if (postTeleportNarrative != null)
        {
            postTeleportNarrative.PlayOnce();
        }
    }
}