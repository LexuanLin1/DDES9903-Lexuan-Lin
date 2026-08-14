using System.Collections;
using UnityEngine;

public sealed class A2EnergyCoreReporter : MonoBehaviour
{
    [Header("Collection")]
    [SerializeField, Min(0f)]
    private float disappearDelay = 1f;

    private bool collected;

    public void CollectEnergyCore()
    {
        if (collected)
        {
            return;
        }

        if (A2NarrativeStateManager.Instance == null)
        {
            Debug.LogError(
                "A2NarrativeStateManager was not found.",
                this
            );

            return;
        }

        collected = true;

        A2NarrativeStateManager.Instance.AcceptEnergyCore();

        Debug.Log(
            "A2: Energy Core collected by the player.",
            this
        );

        StartCoroutine(
            HideAfterDelay()
        );
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(
            disappearDelay
        );

        gameObject.SetActive(false);
    }
}