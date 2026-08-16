using System.Collections;
using TMPro;
using UnityEngine;

public sealed class AccessLogsController : MonoBehaviour
{
    [Header("Screen")]
    [SerializeField] private TMP_Text logText;

    [Header("Voice")]
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioClip restoredRecordVoice;
    [SerializeField] private AudioClip energyCoreRequiredVoice;
    [SerializeField] private AudioClip captainReflectionVoice;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float restoringDuration = 1.5f;
    [SerializeField, Min(0f)] private float timestampDuration = 1.5f;
    [SerializeField, Min(0f)] private float reflectionDelay = 5f;

    private bool isRunning;
    private bool recordRestored;

    public void StartAccessLogs()
    {
        if (isRunning)
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

        if (!A2NarrativeStateManager.Instance.HasEnergyCore)
        {
            ShowEnergyCoreRequired();
            return;
        }

        if (recordRestored)
        {
            ShowFinalRecord();
            return;
        }

        StartCoroutine(
            PlayAccessLogsSequence()
        );
    }

    private IEnumerator PlayAccessLogsSequence()
    {
        isRunning = true;

        // Step 1: Restore archived record.
        if (logText != null)
        {
            logText.text =
                "ACCESS LOGS\n\n" +
                "RESTORING ARCHIVED RECORD...";
        }

        yield return new WaitForSeconds(
            restoringDuration
        );

        // Step 2: Show the timestamp.
        if (logText != null)
        {
            logText.text =
                "SYSTEM MODIFICATION RECORD\n\n" +
                "TIMESTAMP: 5 YEARS AGO";
        }

        yield return new WaitForSeconds(
            timestampDuration
        );

        // Step 3: Show the restored cryosleep record.
        ShowFinalRecord();

        // Step 4: System reads the record.
        PlayVoice(
            restoredRecordVoice
        );

        if (restoredRecordVoice != null)
        {
            yield return new WaitForSeconds(
                restoredRecordVoice.length
            );
        }

        // Step 5: Give the Captain time to process the information.
        yield return new WaitForSeconds(
            reflectionDelay
        );

        // Step 6: Captain reflects on the incomplete information.
        PlayVoice(
            captainReflectionVoice
        );

        if (captainReflectionVoice != null)
        {
            yield return new WaitForSeconds(
                captainReflectionVoice.length
            );
        }

        recordRestored = true;
        isRunning = false;

        Debug.Log(
            "A2: ACCESS LOGS sequence completed.",
            this
        );
    }

    private void ShowFinalRecord()
    {
        if (logText == null)
        {
            return;
        }

        logText.text =
            "CRYOSLEEP POWER REALLOCATION\n\n" +

            "POD 02 & POD 03\n" +
            "REMAINING POWER TRANSFERRED TO:\n\n" +

            "POD 01 - CAPTAIN\n\n" +

            "ESTIMATED REMAINING SUSPENSION:\n" +
            "5 YEARS";
    }

    private void ShowEnergyCoreRequired()
    {
        if (logText != null)
        {
            logText.text =
                "ARCHIVE ACCESS UNAVAILABLE\n\n" +
                "ENERGY CORE REQUIRED";
        }

        PlayVoice(
            energyCoreRequiredVoice
        );

        Debug.Log(
            "A2: ACCESS LOGS unavailable. Energy Core required.",
            this
        );
    }

    private void PlayVoice(AudioClip clip)
    {
        if (voiceSource == null ||
            clip == null)
        {
            return;
        }

        voiceSource.Stop();
        voiceSource.clip = clip;
        voiceSource.loop = false;
        voiceSource.Play();
    }
}