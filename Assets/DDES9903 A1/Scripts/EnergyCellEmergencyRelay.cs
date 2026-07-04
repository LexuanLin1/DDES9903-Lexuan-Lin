using UnityEngine;

public sealed class EnergyCellEmergencyRelay : MonoBehaviour
{
    [Header("Runtime Reference")]
    public EnergyCellEndingController endingController;

    private bool emergencyTriggered;

    private void Awake()
    {
        FindEndingController();
    }

    public void NotifyPickedUp()
    {
        if (emergencyTriggered)
        {
            return;
        }

        if (endingController == null)
        {
            FindEndingController();
        }

        if (endingController == null)
        {
            Debug.LogWarning(
                "EnergyCell could not find EnergyCellEndingController.",
                this
            );

            return;
        }

        emergencyTriggered = true;

        endingController.StartEmergency();

        Debug.Log(
            "EnergyCell pickup detected. Return emergency started.",
            this
        );
    }

    private void FindEndingController()
    {
        endingController =
            FindFirstObjectByType<EnergyCellEndingController>();

        if (endingController == null)
        {
            Debug.LogWarning(
                "No active EnergyCellEndingController exists in the scene.",
                this
            );
        }
    }
}