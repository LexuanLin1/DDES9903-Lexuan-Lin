using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class CryoEnergyCellEndingTrigger : MonoBehaviour
{
    [Header("Ending Controller")]
    public EnergyCellEndingController endingController;

    private bool endingRequested;

    private void Reset()
    {
        Collider triggerCollider =
            GetComponent<Collider>();

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryTriggerEnding(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryTriggerEnding(other);
    }

    private void TryTriggerEnding(Collider other)
    {
        if (endingRequested ||
            endingController == null ||
            endingController.EndingTriggered ||
            !endingController.EmergencyActive)
        {
            return;
        }

        EnergyCellEmergencyRelay energyCell =
            FindEnergyCell(other);

        if (energyCell == null)
        {
            return;
        }

        endingRequested = true;

        endingController.TriggerEnding();

        Debug.Log(
            "EnergyCell reached the cryosleep bay. Ending triggered.",
            this
        );
    }

    private static EnergyCellEmergencyRelay FindEnergyCell(
        Collider other)
    {
        if (other == null)
        {
            return null;
        }

        EnergyCellEmergencyRelay energyCell =
            other.GetComponentInParent<
                EnergyCellEmergencyRelay
            >();

        if (energyCell != null)
        {
            return energyCell;
        }

        energyCell =
            other.GetComponentInChildren<
                EnergyCellEmergencyRelay
            >(true);

        if (energyCell != null)
        {
            return energyCell;
        }

        Rigidbody attachedRigidbody =
            other.attachedRigidbody;

        if (attachedRigidbody == null)
        {
            return null;
        }

        energyCell =
            attachedRigidbody.GetComponentInParent<
                EnergyCellEmergencyRelay
            >();

        if (energyCell != null)
        {
            return energyCell;
        }

        return attachedRigidbody
            .GetComponentInChildren<
                EnergyCellEmergencyRelay
            >(true);
    }
}