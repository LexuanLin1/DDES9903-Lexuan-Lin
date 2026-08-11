using System.Collections;
using TMPro;
using UnityEngine;

public sealed class CryoSequenceController : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private CanvasGroup blackOverlay;

    [Header("Wake Button")]
    [SerializeField] private GameObject wakeButtonYes;

    [Header("Cryo Chamber")]
    [SerializeField] private GameObject doorToHide;
    [SerializeField] private GameObject cryoExitBlocker;

    [SerializeField, Min(0f)]
    private float doorHideDelay = 0.8f;

    [Header("Voice Source")]
    [SerializeField] private AudioSource voiceSource;

    [Header("Voice Clips")]
    [SerializeField] private AudioClip awakeningVoice;
    [SerializeField] private AudioClip timeOverrunVoice;
    [SerializeField] private AudioClip powerCriticalVoice;
    [SerializeField] private AudioClip homeBeaconVoice;
    [SerializeField] private AudioClip wakePromptVoice;
    [SerializeField] private AudioClip wakeConfirmedVoice;
    [SerializeField] private AudioClip cockpitInstructionVoice;

    [Header("Timing")]
    [SerializeField, Min(0f)]
    private float openingBlackDuration = 1.5f;

    [SerializeField, Min(0f)]
    private float messageGap = 0.35f;

    [SerializeField, Min(0.5f)]
    private float fallbackClipDuration = 3f;

    private bool wakeChoiceAvailable;
    private bool wakeConfirmed;

    private void Awake()
    {
        wakeChoiceAvailable = false;
        wakeConfirmed = false;

        if (blackOverlay != null)
        {
            blackOverlay.alpha = 1f;
            blackOverlay.interactable = false;
            blackOverlay.blocksRaycasts = false;
        }

        if (wakeButtonYes != null)
        {
            wakeButtonYes.SetActive(false);
        }

        if (doorToHide != null)
        {
            doorToHide.SetActive(true);
        }

        if (cryoExitBlocker != null)
        {
            cryoExitBlocker.SetActive(true);
        }

        SetDisplay(
            string.Empty,
            new Color32(66, 210, 225, 255)
        );
    }

    private void Start()
    {
        StartCoroutine(BeginWakeSequence());
    }

    private IEnumerator BeginWakeSequence()
    {
        yield return new WaitForSeconds(openingBlackDuration);

        SetDisplay(
            "CRYOSLEEP SYSTEM RESTARTING",
            new Color32(66, 210, 225, 255)
        );

        PlayVoice(awakeningVoice);

        float awakeningDuration =
            GetClipDuration(awakeningVoice, 3f);

        float firstFadeDuration =
            Mathf.Min(2.2f, awakeningDuration);

        yield return StartCoroutine(
            FadeOverlay(
                1f,
                0.42f,
                firstFadeDuration
            )
        );

        yield return WaitForRemainingDuration(
            awakeningDuration,
            firstFadeDuration
        );

        yield return new WaitForSeconds(messageGap);

        SetDisplay(
            "MISSION OVERRUN DETECTED\n\n" +
            "PLANNED JOURNEY: 5 YEARS\n\n" +
            "ACTUAL ELAPSED TIME: 10 YEARS",
            new Color32(245, 177, 66, 255)
        );

        PlayVoice(timeOverrunVoice);

        float timeOverrunDuration =
            GetClipDuration(timeOverrunVoice, 7f);

        float secondFadeDuration =
            Mathf.Min(1.5f, timeOverrunDuration);

        yield return StartCoroutine(
            FadeOverlay(
                0.42f,
                0f,
                secondFadeDuration
            )
        );

        yield return WaitForRemainingDuration(
            timeOverrunDuration,
            secondFadeDuration
        );

        yield return new WaitForSeconds(messageGap);

        yield return StartCoroutine(
            ShowMessageAndWait(
                "CRYOSLEEP POWER: 4%\n\n" +
                "CONTINUED SUSPENSION UNAVAILABLE",
                new Color32(255, 72, 82, 255),
                powerCriticalVoice,
                5f
            )
        );

        yield return StartCoroutine(
            ShowMessageAndWait(
                "HOME BEACON DETECTED\n\n" +
                "SIGNAL STATUS: WEAK\n\n" +
                "DESTINATION: EIRENE",
                new Color32(104, 225, 170, 255),
                homeBeaconVoice,
                5f
            )
        );

        SetDisplay(
            "MANUAL AWAKENING REQUIRED\n\n" +
            "WAKE UP TO CONTINUE THE JOURNEY HOME",
            new Color32(66, 210, 225, 255)
        );

        PlayVoice(wakePromptVoice);

        yield return new WaitForSeconds(
            GetClipDuration(wakePromptVoice, 5f)
        );

        wakeChoiceAvailable = true;

        if (wakeButtonYes != null)
        {
            wakeButtonYes.SetActive(true);
        }
    }

    public void OnWakeYesPressed()
    {
        Debug.Log(
            $"YES pressed. Choice available: {wakeChoiceAvailable}",
            this
        );

        if (!wakeChoiceAvailable || wakeConfirmed)
        {
            return;
        }

        wakeChoiceAvailable = false;
        wakeConfirmed = true;

        if (wakeButtonYes != null)
        {
            wakeButtonYes.SetActive(false);
        }

        StartCoroutine(ConfirmWake());
    }

    private IEnumerator ConfirmWake()
    {
        SetDisplay(
            "AWAKENING CONFIRMED\n\n" +
            "HOMEWARD JOURNEY RESUMED",
            new Color32(75, 230, 145, 255)
        );

        PlayVoice(wakeConfirmedVoice);

        float confirmedDuration =
            GetClipDuration(wakeConfirmedVoice, 6f);

        float actualDoorDelay =
            Mathf.Min(doorHideDelay, confirmedDuration);

        yield return new WaitForSeconds(actualDoorDelay);

        if (doorToHide != null)
        {
            doorToHide.SetActive(false);
        }
        else
        {
            Debug.LogWarning(
                "Door To Hide has not been assigned.",
                this
            );
        }

        if (cryoExitBlocker != null)
        {
            cryoExitBlocker.SetActive(false);
        }
        else
        {
            Debug.LogWarning(
                "Cryo Exit Blocker has not been assigned.",
                this
            );
        }

        yield return WaitForRemainingDuration(
            confirmedDuration,
            actualDoorDelay
        );

        yield return new WaitForSeconds(messageGap);

        SetDisplay(
            "PROCEED TO THE COCKPIT\n\n" +
            "FURTHER MISSION INSTRUCTIONS WAITING",
            new Color32(104, 225, 170, 255)
        );

        PlayVoice(cockpitInstructionVoice);
    }

    private IEnumerator ShowMessageAndWait(
        string message,
        Color colour,
        AudioClip clip,
        float fallbackDuration)
    {
        SetDisplay(message, colour);
        PlayVoice(clip);

        yield return new WaitForSeconds(
            GetClipDuration(
                clip,
                fallbackDuration
            )
        );

        yield return new WaitForSeconds(messageGap);
    }

    private IEnumerator FadeOverlay(
        float startAlpha,
        float endAlpha,
        float duration)
    {
        if (blackOverlay == null)
        {
            yield break;
        }

        if (duration <= 0f)
        {
            blackOverlay.alpha = endAlpha;
            yield break;
        }

        float elapsedTime = 0f;
        blackOverlay.alpha = startAlpha;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsedTime / duration);

            float smoothProgress =
                Mathf.SmoothStep(0f, 1f, progress);

            blackOverlay.alpha =
                Mathf.Lerp(
                    startAlpha,
                    endAlpha,
                    smoothProgress
                );

            yield return null;
        }

        blackOverlay.alpha = endAlpha;
    }

    private IEnumerator WaitForRemainingDuration(
        float totalDuration,
        float elapsedDuration)
    {
        float remainingDuration =
            totalDuration - elapsedDuration;

        if (remainingDuration > 0f)
        {
            yield return new WaitForSeconds(
                remainingDuration
            );
        }
    }

    private float GetClipDuration(
        AudioClip clip,
        float fallbackDuration)
    {
        if (clip != null && clip.length > 0f)
        {
            return clip.length;
        }

        return Mathf.Max(
            fallbackDuration,
            fallbackClipDuration
        );
    }

    private void PlayVoice(AudioClip clip)
    {
        if (voiceSource == null || clip == null)
        {
            return;
        }

        voiceSource.Stop();
        voiceSource.clip = clip;
        voiceSource.loop = false;
        voiceSource.Play();
    }

    private void SetDisplay(
        string message,
        Color colour)
    {
        if (statusText == null)
        {
            return;
        }

        statusText.text = message;
        statusText.color = Color.black;
    }

    [ContextMenu("TEST YES")]
    private void TestYes()
    {
        wakeChoiceAvailable = true;
        OnWakeYesPressed();
    }
}