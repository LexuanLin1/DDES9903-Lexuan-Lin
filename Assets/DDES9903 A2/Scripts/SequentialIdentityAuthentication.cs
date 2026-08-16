using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum IdentityColourStage
{
    Red,
    Green,
    Blue
}

public sealed class SequentialIdentityAuthentication : MonoBehaviour
{
    [Header("Security System")]
    [SerializeField] private MissionSecurityController securityController;
    [SerializeField] private AudioSource alarmSource;

    [SerializeField, Range(0f, 1f)]
    private float alarmDialogueVolume = 0.08f;

    [Header("Memory Stage Roots")]
    [SerializeField] private GameObject redStageRoot;
    [SerializeField] private GameObject greenStageRoot;
    [SerializeField] private GameObject blueStageRoot;

    [Header("Completed Visuals")]
    [SerializeField] private GameObject redCompletedVisual;
    [SerializeField] private GameObject greenCompletedVisual;
    [SerializeField] private GameObject blueCompletedVisual;

    [Header("Final Decision")]
    [SerializeField] private GameObject finalCommandRoot;
    [SerializeField] private FinalDecisionController finalDecisionController;

    [Header("Display")]
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private Color displayTextColour = Color.white;

    [Header("Photo Display")]
    [SerializeField] private Image memoryPhotoImage;
    [SerializeField] private Sprite redMemoryPhoto;
    [SerializeField] private Sprite greenMemoryPhoto;
    [SerializeField] private Sprite blueMemoryPhoto;

    [Header("Voice Sources")]
    [SerializeField] private AudioSource securityVoiceSource;
    [SerializeField] private AudioSource memoryVoiceSource;
    [SerializeField] private AudioSource playerVoiceSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Voice Clips")]
    [SerializeField] private AudioClip arrivalRecallVoice;
    [SerializeField] private AudioClip redMemoryVoice;
    [SerializeField] private AudioClip greenMemoryVoice;
    [SerializeField] private AudioClip blueMemoryVoice;
    [SerializeField] private AudioClip finalInstructionVoice;

    [Header("SFX")]
    [SerializeField] private AudioClip stageSuccessSfx;

    [Header("Shared Subtitles")]
    [SerializeField] private CanvasGroup subtitleGroup;
    [SerializeField] private TMP_Text subtitleText;

    [TextArea(2, 5)]
    [SerializeField]
    private string arrivalRecallSubtitle =
        "Final command protocol.\n" +
        "Three preserved records detected:\n" +
        "Family, Earth, and Crew.";

    [TextArea(2, 5)]
    [SerializeField]
    private string redMemorySubtitle =
        "Dad, promise you'll come back.\n" +
        "I'll be waiting for you.";

    [TextArea(2, 5)]
    [SerializeField]
    private string greenMemorySubtitle =
        "Home...\n" +
        "After ten years, it's still right there.";

    [TextArea(2, 5)]
    [SerializeField]
    private string blueMemorySubtitle =
        "Whatever happens, Captain...\n" +
        "don't let our story disappear with this ship.";

    [TextArea(2, 5)]
    [SerializeField]
    private string finalInstructionSubtitle =
        "Memory review complete.\n" +
        "Final command interface unlocked.";

    [Header("Subtitle Colours")]
    [SerializeField] private Color playerSubtitleColour = Color.white;
    [SerializeField] private Color memorySubtitleColour = Color.white;

    [SerializeField]
    private Color securitySubtitleColour =
        new Color(1f, 0.75f, 0.75f, 1f);

    [Header("Timing")]
    [SerializeField, Min(0f)]
    private float arrivalSafetyDelay = 0.3f;

    [SerializeField, Min(0f)]
    private float delayBeforeVoice = 0.25f;

    [SerializeField, Min(0f)]
    private float finalDecisionDelay = 1f;

    [SerializeField, Min(0f)]
    private float subtitleFadeDuration = 0.2f;

    [SerializeField, Min(0f)]
    private float subtitleExtraTime = 0.2f;

    [SerializeField, Min(0.5f)]
    private float fallbackVoiceDuration = 4f;

    private bool sequenceStarted;
    private bool sequenceBusy;
    private bool sequenceComplete;

    private bool redCompleted;
    private bool greenCompleted;
    private bool blueCompleted;

    private float originalAlarmVolume;
    private bool alarmVolumeLowered;

    private void Awake()
    {
        sequenceStarted = false;
        sequenceBusy = false;
        sequenceComplete = false;

        redCompleted = false;
        greenCompleted = false;
        blueCompleted = false;

        SetObjectActive(redStageRoot, false);
        SetObjectActive(greenStageRoot, false);
        SetObjectActive(blueStageRoot, false);

        SetObjectActive(redCompletedVisual, false);
        SetObjectActive(greenCompletedVisual, false);
        SetObjectActive(blueCompletedVisual, false);

        // Final decision remains hidden until all memories are reviewed.
        SetObjectActive(finalCommandRoot, false);

        if (subtitleGroup != null)
        {
            subtitleGroup.alpha = 0f;
            subtitleGroup.interactable = false;
            subtitleGroup.blocksRaycasts = false;
        }

        HideMemoryPhoto();

        SetDisplay(
            "FINAL COMMAND ARCHIVE\n\n" +
            "AWAITING CAPTAIN"
        );
    }

    public void BeginFromArrival()
    {
        if (sequenceStarted)
        {
            return;
        }

        sequenceStarted = true;

        StartCoroutine(
            BeginArrivalSequence()
        );
    }

    public bool CanValidateStage(
        IdentityColourStage colourStage)
    {
        if (!sequenceStarted ||
            sequenceBusy ||
            sequenceComplete)
        {
            return false;
        }

        return colourStage switch
        {
            IdentityColourStage.Red => !redCompleted,
            IdentityColourStage.Green => !greenCompleted,
            IdentityColourStage.Blue => !blueCompleted,
            _ => false
        };
    }

    public void ReportRedCorrect()
    {
        if (!CanValidateStage(
            IdentityColourStage.Red))
        {
            return;
        }

        StartCoroutine(
            CompleteRedStage()
        );
    }

    public void ReportGreenCorrect()
    {
        if (!CanValidateStage(
            IdentityColourStage.Green))
        {
            return;
        }

        StartCoroutine(
            CompleteGreenStage()
        );
    }

    public void ReportBlueCorrect()
    {
        if (!CanValidateStage(
            IdentityColourStage.Blue))
        {
            return;
        }

        StartCoroutine(
            CompleteBlueStage()
        );
    }

    public void ReportIncorrectAttempt()
    {
        if (!sequenceStarted ||
            sequenceBusy ||
            sequenceComplete)
        {
            return;
        }

        SetDisplay(
            "MEMORY RECORD ALREADY REVIEWED\n\n" +
            "SELECT ANOTHER RECORD"
        );
    }

    private IEnumerator BeginArrivalSequence()
    {
        sequenceBusy = true;

        LowerAlarmVolume();

        yield return StartCoroutine(
            WaitForDialogueSourcesToFinish()
        );

        yield return new WaitForSeconds(
            arrivalSafetyDelay
        );

        SetDisplay(
            "FINAL COMMAND PROTOCOL\n\n" +
            "3 PRESERVED RECORDS DETECTED\n\n" +
            "FAMILY   EARTH   CREW"
        );

        yield return StartCoroutine(
            PlayDialogue(
                securityVoiceSource,
                arrivalRecallVoice,
                arrivalRecallSubtitle,
                securitySubtitleColour
            )
        );

        // All three memories are available in any order.
        SetObjectActive(
            redStageRoot,
            true
        );

        SetObjectActive(
            greenStageRoot,
            true
        );

        SetObjectActive(
            blueStageRoot,
            true
        );

        sequenceBusy = false;
    }

    private IEnumerator CompleteRedStage()
    {
        sequenceBusy = true;

        SetObjectActive(
            redStageRoot,
            false
        );

        yield return StartCoroutine(
            PlaySfxAndWait(
                stageSuccessSfx
            )
        );

        redCompleted = true;

        SetObjectActive(
            redCompletedVisual,
            true
        );

        SetDisplay(
            "FAMILY RECORD REVIEWED\n\n" +
            GetProgressText()
        );

        ShowMemoryPhoto(
            redMemoryPhoto
        );

        yield return new WaitForSeconds(
            delayBeforeVoice
        );

        yield return StartCoroutine(
            PlayDialogue(
                memoryVoiceSource,
                redMemoryVoice,
                redMemorySubtitle,
                memorySubtitleColour
            )
        );

        HideMemoryPhoto();

        yield return StartCoroutine(
            CheckForCompletion()
        );

        sequenceBusy = false;
    }

    private IEnumerator CompleteGreenStage()
    {
        sequenceBusy = true;

        SetObjectActive(
            greenStageRoot,
            false
        );

        yield return StartCoroutine(
            PlaySfxAndWait(
                stageSuccessSfx
            )
        );

        greenCompleted = true;

        SetObjectActive(
            greenCompletedVisual,
            true
        );

        SetDisplay(
            "EARTH RECORD REVIEWED\n\n" +
            GetProgressText()
        );

        ShowMemoryPhoto(
            greenMemoryPhoto
        );

        yield return new WaitForSeconds(
            delayBeforeVoice
        );

        yield return StartCoroutine(
            PlayDialogue(
                playerVoiceSource,
                greenMemoryVoice,
                greenMemorySubtitle,
                playerSubtitleColour
            )
        );

        HideMemoryPhoto();

        yield return StartCoroutine(
            CheckForCompletion()
        );

        sequenceBusy = false;
    }

    private IEnumerator CompleteBlueStage()
    {
        sequenceBusy = true;

        SetObjectActive(
            blueStageRoot,
            false
        );

        yield return StartCoroutine(
            PlaySfxAndWait(
                stageSuccessSfx
            )
        );

        blueCompleted = true;

        SetObjectActive(
            blueCompletedVisual,
            true
        );

        SetDisplay(
            "CREW RECORD REVIEWED\n\n" +
            GetProgressText()
        );

        ShowMemoryPhoto(
            blueMemoryPhoto
        );

        yield return new WaitForSeconds(
            delayBeforeVoice
        );

        yield return StartCoroutine(
            PlayDialogue(
                memoryVoiceSource,
                blueMemoryVoice,
                blueMemorySubtitle,
                memorySubtitleColour
            )
        );

        HideMemoryPhoto();

        yield return StartCoroutine(
            CheckForCompletion()
        );

        sequenceBusy = false;
    }

    private IEnumerator CheckForCompletion()
    {
        if (!redCompleted ||
            !greenCompleted ||
            !blueCompleted)
        {
            SetDisplay(
                "FINAL COMMAND ARCHIVE\n\n" +
                GetProgressText() +
                "\n\nREVIEW REMAINING RECORDS"
            );

            yield break;
        }

        yield return StartCoroutine(
            FinaliseSequence()
        );
    }

    private IEnumerator FinaliseSequence()
    {
        if (sequenceComplete)
        {
            yield break;
        }

        sequenceComplete = true;

        SetDisplay(
            "MEMORY REVIEW COMPLETE\n\n" +
            "FINAL COMMAND ACCESS RESTORED"
        );

        // Stop / complete the old security alarm system.
        CompleteSecurityAuthentication();

        // Make the final decision objects available.
        SetObjectActive(
            finalCommandRoot,
            true
        );

        // Final archive confirmation.
        yield return StartCoroutine(
            PlayDialogue(
                securityVoiceSource,
                finalInstructionVoice,
                finalInstructionSubtitle,
                securitySubtitleColour
            )
        );

        // Small pause before beginning the final decision.
        yield return new WaitForSeconds(
            finalDecisionDelay
        );

        StartFinalDecision();

        Debug.Log(
            "A2: Memory review completed. Final decision sequence started.",
            this
        );
    }

    private void StartFinalDecision()
    {
        FinalDecisionController controller =
            finalDecisionController;

        // Fallback: automatically find the controller
        // on FinalCommandRoot if it was not manually assigned.
        if (controller == null &&
            finalCommandRoot != null)
        {
            controller =
                finalCommandRoot
                    .GetComponent<FinalDecisionController>();
        }

        if (controller == null)
        {
            Debug.LogError(
                "FinalDecisionController was not found.",
                this
            );

            return;
        }

        controller.BeginFinalDecision();
    }

    private IEnumerator PlayDialogue(
        AudioSource source,
        AudioClip clip,
        string message,
        Color subtitleColour)
    {
        yield return StartCoroutine(
            WaitForDialogueSourcesToFinish()
        );

        if (subtitleText != null)
        {
            subtitleText.text = message;
            subtitleText.color = subtitleColour;
        }

        yield return StartCoroutine(
            FadeSubtitle(
                0f,
                1f
            )
        );

        if (source != null &&
            clip != null)
        {
            source.Stop();
            source.clip = clip;
            source.loop = false;
            source.Play();
        }

        float duration =
            GetClipDuration(
                clip
            );

        yield return new WaitForSeconds(
            duration +
            subtitleExtraTime
        );

        yield return StartCoroutine(
            FadeSubtitle(
                1f,
                0f
            )
        );
    }

    private IEnumerator WaitForDialogueSourcesToFinish()
    {
        while (IsAnyDialogueSourcePlaying())
        {
            yield return null;
        }
    }

    private bool IsAnyDialogueSourcePlaying()
    {
        return
            IsPlaying(securityVoiceSource) ||
            IsPlaying(memoryVoiceSource) ||
            IsPlaying(playerVoiceSource);
    }

    private static bool IsPlaying(
        AudioSource source)
    {
        return
            source != null &&
            source.isPlaying;
    }

    private IEnumerator PlaySfxAndWait(
        AudioClip clip)
    {
        if (sfxSource == null ||
            clip == null)
        {
            yield break;
        }

        sfxSource.Stop();
        sfxSource.clip = clip;
        sfxSource.loop = false;
        sfxSource.Play();

        yield return new WaitForSeconds(
            clip.length
        );
    }

    private IEnumerator FadeSubtitle(
        float startAlpha,
        float endAlpha)
    {
        if (subtitleGroup == null)
        {
            yield break;
        }

        if (subtitleFadeDuration <= 0f)
        {
            subtitleGroup.alpha =
                endAlpha;

            yield break;
        }

        float elapsedTime = 0f;

        subtitleGroup.alpha =
            startAlpha;

        while (elapsedTime <
               subtitleFadeDuration)
        {
            elapsedTime +=
                Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsedTime /
                    subtitleFadeDuration
                );

            subtitleGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    endAlpha,
                    progress
                );

            yield return null;
        }

        subtitleGroup.alpha =
            endAlpha;
    }

    private float GetClipDuration(
        AudioClip clip)
    {
        if (clip != null &&
            clip.length > 0f)
        {
            return clip.length;
        }

        return fallbackVoiceDuration;
    }

    private void ShowMemoryPhoto(
        Sprite sprite)
    {
        if (memoryPhotoImage == null)
        {
            return;
        }

        memoryPhotoImage.sprite =
            sprite;

        memoryPhotoImage.enabled =
            sprite != null;
    }

    private void HideMemoryPhoto()
    {
        if (memoryPhotoImage == null)
        {
            return;
        }

        memoryPhotoImage.sprite =
            null;

        memoryPhotoImage.enabled =
            false;
    }

    private string GetProgressText()
    {
        int completed = 0;

        if (redCompleted)
        {
            completed++;
        }

        if (greenCompleted)
        {
            completed++;
        }

        if (blueCompleted)
        {
            completed++;
        }

        return
            "RECORDS REVIEWED: " +
            completed +
            " / 3";
    }

    private void CompleteSecurityAuthentication()
    {
        if (alarmSource != null &&
            alarmVolumeLowered)
        {
            alarmSource.volume =
                originalAlarmVolume;
        }

        alarmVolumeLowered = false;

        if (securityController != null)
        {
            securityController
                .CompleteAuthentication();
        }
        else if (alarmSource != null)
        {
            alarmSource.Stop();
        }
    }

    private void LowerAlarmVolume()
    {
        if (alarmSource == null ||
            alarmVolumeLowered)
        {
            return;
        }

        originalAlarmVolume =
            alarmSource.volume;

        alarmSource.volume =
            Mathf.Min(
                originalAlarmVolume,
                alarmDialogueVolume
            );

        alarmVolumeLowered = true;
    }

    private void SetDisplay(
        string message)
    {
        if (displayText == null)
        {
            return;
        }

        displayText.text =
            message;

        displayText.color =
            displayTextColour;
    }

    private static void SetObjectActive(
        GameObject target,
        bool active)
    {
        if (target != null)
        {
            target.SetActive(
                active
            );
        }
    }

    private void OnDisable()
    {
        if (alarmSource != null &&
            alarmVolumeLowered)
        {
            alarmSource.volume =
                originalAlarmVolume;
        }
    }
}