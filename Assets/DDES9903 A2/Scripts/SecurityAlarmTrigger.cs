using UnityEngine;

public sealed class SecurityAlarmTrigger : MonoBehaviour
{
    [SerializeField]
    private MissionSecurityController securityController;

    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || securityController == null)
        {
            return;
        }

        CharacterController player =
            other.GetComponentInParent<CharacterController>();

        if (player == null)
        {
            return;
        }

        hasTriggered = true;
        securityController.BeginSecurityProtocol();
    }
}
