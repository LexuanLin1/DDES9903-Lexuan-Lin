using System.Collections;
using UnityEngine;

public sealed class A2EnergyCoreReporter : MonoBehaviour
{
    [Header("Collection")]
    [SerializeField, Min(0f)]
    private float disappearDelay = 1f;

    [Header("System Voice")]
    [SerializeField] private AudioSource systemVoiceSource;
    [SerializeField] private AudioClip manualControlAuthorizedVoice;

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

        PlaySystemVoice();

        Debug.Log(
            "A2: Energy Core collected by the player.",
            this
        );

        StartCoroutine(HideAfterDelay());
    }

    private void PlaySystemVoice()
    {
        if (systemVoiceSource == null ||
            manualControlAuthorizedVoice == null)
        {
            return;
        }

        systemVoiceSource.Stop();
        systemVoiceSource.clip =
            manualControlAuthorizedVoice;

        systemVoiceSource.loop = false;
        systemVoiceSource.Play();
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(
            disappearDelay
        );

        gameObject.SetActive(false);
    }
}