using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class IdentityHallArrivalTrigger : MonoBehaviour
{
    [SerializeField]
    private SequentialIdentityAuthentication identitySequence;

    private bool hasTriggered;

    private void Reset()
    {
        Collider triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || identitySequence == null)
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

        hasTriggered = true;
        identitySequence.BeginFromArrival();

        Collider triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }
    }
}
