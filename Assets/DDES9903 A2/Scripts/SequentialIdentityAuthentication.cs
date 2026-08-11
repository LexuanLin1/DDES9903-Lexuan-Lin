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
    private enum SequenceStage
    {
        WaitingForArrival,
        Red,
        Green,
        Blue,
        Finalising,
        Complete
    }

    [Header("Security System")]
    [SerializeField] private MissionSecurityController securityController;
    [SerializeField] private AudioSource alarmSource;
    [SerializeField, Range(0f, 1f)] private float alarmDialogueVolume = 0.08f;

    [Header("Stage Roots")]
    [SerializeField] private GameObject redStageRoot;
    [SerializeField] private GameObject greenStageRoot;
    [SerializeField] private GameObject blueStageRoot;

    [Header("Completed Visuals")]
    [SerializeField] private GameObject redCompletedVisual;
    [SerializeField] private GameObject greenCompletedVisual;
    [SerializeField] private GameObject blueCompletedVisual;

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
    [SerializeField] private AudioClip crewLogVoice;
    [SerializeField] private AudioClip playerResponseVoice;
    [SerializeField] private AudioClip finalInstructionVoice;

    [Header("SFX")]
    [SerializeField] private AudioClip stageSuccessSfx;
    [SerializeField] private AudioClip stageFailureSfx;

    [Header("Shared Subtitles")]
    [SerializeField] private CanvasGroup subtitleGroup;
    [SerializeField] private TMP_Text subtitleText;

    [TextArea(2, 4)]
    [SerializeField]
    private string arrivalRecallSubtitle =
        "This array... right.\n" +
        "Red for family, green for Earth, blue for crew.";

    [TextArea(2, 4)]
    [SerializeField]
    private string redMemorySubtitle =
        "Come back to Earth.\n" +
        "The garden will be green when you return.";

    [TextArea(2, 4)]
    [SerializeField]
    private string greenMemorySubtitle =
        "Earth memory recovered.\n" +
        "Green land... blue oceans... home.";

    [TextArea(2, 4)]
    [SerializeField]
    private string blueMemorySubtitle =
        "Crew memory recovered.\n" +
        "Explorer profile confirmed.";

    [TextArea(2, 4)]
    [SerializeField]
    private string crewLogSubtitle =
        "Emergency log.\n" +
        "We transferred our remaining power to Pod One.\n" +
        "One of us must make it back to Earth.";

    [TextArea(2, 4)]
    [SerializeField]
    private string playerResponseSubtitle =
        "They gave me their power...\n" +
        "I won't fail them. I'll bring us home.";

    [TextArea(2, 4)]
    [SerializeField]
    private string finalInstructionSubtitle =
        "Identity verified.\n" +
        "Retrieve the energy crystal and return to the lower cryosleep bay.";

    [Header("Subtitle Colours")]
    [SerializeField] private Color playerSubtitleColour = Color.white;
    [SerializeField] private Color memorySubtitleColour = Color.white;
    [SerializeField]
    private Color securitySubtitleColour =
        new Color(1f, 0.75f, 0.75f, 1f);

    [Header("Timing")]
    [SerializeField, Min(0f)] private float arrivalSafetyDelay = 0.3f;
    [SerializeField, Min(0f)] private float delayBeforeVoice = 0.25f;
    [SerializeField, Min(0f)] private float gapBetweenVoices = 0.5f;
    [SerializeField, Min(0f)] private float subtitleFadeDuration = 0.2f;
    [SerializeField, Min(0f)] private float subtitleExtraTime = 0.2f;
    [SerializeField, Min(0.5f)] private float fallbackVoiceDuration = 4f;

    private SequenceStage currentStage;
    private bool sequenceStarted;
    private bool sequenceBusy;
    private bool incorrectAttemptPlaying;

    private float originalAlarmVolume;
    private bool alarmVolumeLowered;

    private void Awake()
    {
        currentStage = SequenceStage.WaitingForArrival;
        sequenceStarted = false;
        sequenceBusy = false;

        SetObjectActive(redStageRoot, false);
        SetObjectActive(greenStageRoot, false);
        SetObjectActive(blueStageRoot, false);

        SetObjectActive(redCompletedVisual, false);
        SetObjectActive(greenCompletedVisual, false);
        SetObjectActive(blueCompletedVisual, false);

        if (subtitleGroup != null)
        {
            subtitleGroup.alpha = 0f;
            subtitleGroup.interactable = false;
            subtitleGroup.blocksRaycasts = false;
        }

        HideMemoryPhoto();

        SetDisplay(
            "IDENTITY CALIBRATION ARRAY\n\n" +
            "CREW RECORD UNAVAILABLE"
        );
    }

    public void BeginFromArrival()
    {
        if (sequenceStarted)
        {
            return;
        }

        sequenceStarted = true;
        StartCoroutine(BeginArrivalSequence());
    }

    public bool CanValidateStage(IdentityColourStage colourStage)
    {
        if (sequenceBusy)
        {
            return false;
        }

        return colourStage switch
        {
            IdentityColourStage.Red => currentStage == SequenceStage.Red,
            IdentityColourStage.Green => currentStage == SequenceStage.Green,
            IdentityColourStage.Blue => currentStage == SequenceStage.Blue,
            _ => false
        };
    }

    public void ReportRedCorrect()
    {
        if (!CanValidateStage(IdentityColourStage.Red))
        {
            return;
        }

        StartCoroutine(CompleteRedStage());
    }

    public void ReportGreenCorrect()
    {
        if (!CanValidateStage(IdentityColourStage.Green))
        {
            return;
        }

        StartCoroutine(CompleteGreenStage());
    }

    public void ReportBlueCorrect()
    {
        if (!CanValidateStage(IdentityColourStage.Blue))
        {
            return;
        }

        StartCoroutine(CompleteBlueStage());
    }

    public void ReportIncorrectAttempt()
    {
        if (sequenceBusy ||
            incorrectAttemptPlaying ||
            currentStage == SequenceStage.WaitingForArrival ||
            currentStage == SequenceStage.Finalising ||
            currentStage == SequenceStage.Complete)
        {
            return;
        }

        StartCoroutine(IncorrectAttemptSequence());
    }

    private IEnumerator BeginArrivalSequence()
    {
        sequenceBusy = true;

        LowerAlarmVolume();

        yield return StartCoroutine(WaitForDialogueSourcesToFinish());
        yield return new WaitForSeconds(arrivalSafetyDelay);

        SetDisplay(
            "MEMORY ANCHOR 01\n\n" +
            "FAMILY\n\n" +
            "AWAITING INPUT"
        );

        yield return StartCoroutine(
            PlayDialogue(
                playerVoiceSource,
                arrivalRecallVoice,
                arrivalRecallSubtitle,
                playerSubtitleColour
            )
        );

        currentStage = SequenceStage.Red;
        SetObjectActive(redStageRoot, true);

        sequenceBusy = false;
    }

    private IEnumerator CompleteRedStage()
    {
        sequenceBusy = true;

        SetObjectActive(redStageRoot, false);
        yield return StartCoroutine(PlaySfxAndWait(stageSuccessSfx));

        SetObjectActive(redCompletedVisual, true);

        SetDisplay(
            "MEMORY ANCHOR 01 CONFIRMED\n\n" +
            "FAMILY MEMORY RESTORED"
        );

        ShowMemoryPhoto(redMemoryPhoto);

        yield return new WaitForSeconds(delayBeforeVoice);

        yield return StartCoroutine(
            PlayDialogue(
                memoryVoiceSource,
                redMemoryVoice,
                redMemorySubtitle,
                memorySubtitleColour
            )
        );

        HideMemoryPhoto();

        currentStage = SequenceStage.Green;

        SetDisplay(
            "MEMORY ANCHOR 02\n\n" +
            "EARTH\n\n" +
            "AWAITING INPUT"
        );

        SetObjectActive(greenStageRoot, true);

        sequenceBusy = false;
    }

    private IEnumerator CompleteGreenStage()
    {
        sequenceBusy = true;

        SetObjectActive(greenStageRoot, false);
        yield return StartCoroutine(PlaySfxAndWait(stageSuccessSfx));

        SetObjectActive(greenCompletedVisual, true);

        SetDisplay(
            "MEMORY ANCHOR 02 CONFIRMED\n\n" +
            "EARTH MEMORY RESTORED"
        );

        ShowMemoryPhoto(greenMemoryPhoto);

        yield return new WaitForSeconds(delayBeforeVoice);

        yield return StartCoroutine(
            PlayDialogue(
                memoryVoiceSource,
                greenMemoryVoice,
                greenMemorySubtitle,
                memorySubtitleColour
            )
        );

        HideMemoryPhoto();

        currentStage = SequenceStage.Blue;

        SetDisplay(
            "MEMORY ANCHOR 03\n\n" +
            "CREW\n\n" +
            "AWAITING INPUT"
        );

        SetObjectActive(blueStageRoot, true);

        sequenceBusy = false;
    }

    private IEnumerator CompleteBlueStage()
    {
        sequenceBusy = true;
        currentStage = SequenceStage.Finalising;

        SetObjectActive(blueStageRoot, false);
        yield return StartCoroutine(PlaySfxAndWait(stageSuccessSfx));

        SetObjectActive(blueCompletedVisual, true);

        SetDisplay(
            "MEMORY ANCHOR 03 CONFIRMED\n\n" +
            "CREW MEMORY RESTORED"
        );

        ShowMemoryPhoto(blueMemoryPhoto);

        yield return new WaitForSeconds(delayBeforeVoice);

        yield return StartCoroutine(
            PlayDialogue(
                memoryVoiceSource,
                blueMemoryVoice,
                blueMemorySubtitle,
                memorySubtitleColour
            )
        );

        yield return new WaitForSeconds(gapBetweenVoices);

        yield return StartCoroutine(
            PlayDialogue(
                memoryVoiceSource,
                crewLogVoice,
                crewLogSubtitle,
                memorySubtitleColour
            )
        );

        yield return new WaitForSeconds(gapBetweenVoices);

        yield return StartCoroutine(
            PlayDialogue(
                playerVoiceSource,
                playerResponseVoice,
                playerResponseSubtitle,
                playerSubtitleColour
            )
        );

        HideMemoryPhoto();

        SetDisplay(
            "IDENTITY VERIFIED\n\n" +
            "OBJECTIVE: RETRIEVE ENERGY CRYSTAL"
        );

        CompleteSecurityAuthentication();

        yield return StartCoroutine(
            PlayDialogue(
                securityVoiceSource,
                finalInstructionVoice,
                finalInstructionSubtitle,
                securitySubtitleColour
            )
        );

        currentStage = SequenceStage.Complete;
        sequenceBusy = false;
    }

    private IEnumerator IncorrectAttemptSequence()
    {
        incorrectAttemptPlaying = true;
        sequenceBusy = true;

        SetDisplay(
            "MEMORY PATTERN MISMATCH\n\n" +
            "RECALIBRATION REQUIRED"
        );

        yield return StartCoroutine(PlaySfxAndWait(stageFailureSfx));

        yield return new WaitForSeconds(0.25f);

        RestoreCurrentStageDisplay();

        sequenceBusy = false;
        incorrectAttemptPlaying = false;
    }

    private IEnumerator PlayDialogue(
        AudioSource source,
        AudioClip clip,
        string message,
        Color subtitleColour)
    {
        yield return StartCoroutine(WaitForDialogueSourcesToFinish());

        if (subtitleText != null)
        {
            subtitleText.text = message;
            subtitleText.color = subtitleColour;
        }

        yield return StartCoroutine(FadeSubtitle(0f, 1f));

        if (source != null && clip != null)
        {
            source.Stop();
            source.clip = clip;
            source.loop = false;
            source.Play();
        }

        float duration = GetClipDuration(clip);

        yield return new WaitForSeconds(duration + subtitleExtraTime);

        yield return StartCoroutine(FadeSubtitle(1f, 0f));
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
        return IsPlaying(securityVoiceSource) ||
               IsPlaying(memoryVoiceSource) ||
               IsPlaying(playerVoiceSource);
    }

    private static bool IsPlaying(AudioSource source)
    {
        return source != null && source.isPlaying;
    }

    private IEnumerator PlaySfxAndWait(AudioClip clip)
    {
        if (sfxSource == null || clip == null)
        {
            yield break;
        }

        sfxSource.Stop();
        sfxSource.clip = clip;
        sfxSource.loop = false;
        sfxSource.Play();

        yield return new WaitForSeconds(clip.length);
    }

    private IEnumerator FadeSubtitle(float startAlpha, float endAlpha)
    {
        if (subtitleGroup == null)
        {
            yield break;
        }

        if (subtitleFadeDuration <= 0f)
        {
            subtitleGroup.alpha = endAlpha;
            yield break;
        }

        float elapsedTime = 0f;
        subtitleGroup.alpha = startAlpha;

        while (elapsedTime < subtitleFadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsedTime / subtitleFadeDuration);

            subtitleGroup.alpha = Mathf.Lerp(
                startAlpha,
                endAlpha,
                progress
            );

            yield return null;
        }

        subtitleGroup.alpha = endAlpha;
    }

    private float GetClipDuration(AudioClip clip)
    {
        if (clip != null && clip.length > 0f)
        {
            return clip.length;
        }

        return fallbackVoiceDuration;
    }

    private void ShowMemoryPhoto(Sprite sprite)
    {
        if (memoryPhotoImage == null)
        {
            return;
        }

        memoryPhotoImage.sprite = sprite;
        memoryPhotoImage.enabled = sprite != null;
    }

    private void HideMemoryPhoto()
    {
        if (memoryPhotoImage == null)
        {
            return;
        }

        memoryPhotoImage.sprite = null;
        memoryPhotoImage.enabled = false;
    }

    private void CompleteSecurityAuthentication()
    {
        if (alarmSource != null && alarmVolumeLowered)
        {
            alarmSource.volume = originalAlarmVolume;
        }

        alarmVolumeLowered = false;

        if (securityController != null)
        {
            securityController.CompleteAuthentication();
        }
        else if (alarmSource != null)
        {
            alarmSource.Stop();
        }
    }

    private void LowerAlarmVolume()
    {
        if (alarmSource == null || alarmVolumeLowered)
        {
            return;
        }

        originalAlarmVolume = alarmSource.volume;
        alarmSource.volume = Mathf.Min(originalAlarmVolume, alarmDialogueVolume);
        alarmVolumeLowered = true;
    }

    private void RestoreCurrentStageDisplay()
    {
        switch (currentStage)
        {
            case SequenceStage.Red:
                SetDisplay(
                    "MEMORY ANCHOR 01\n\n" +
                    "FAMILY\n\n" +
                    "AWAITING INPUT"
                );
                break;

            case SequenceStage.Green:
                SetDisplay(
                    "MEMORY ANCHOR 02\n\n" +
                    "EARTH\n\n" +
                    "AWAITING INPUT"
                );
                break;

            case SequenceStage.Blue:
                SetDisplay(
                    "MEMORY ANCHOR 03\n\n" +
                    "CREW\n\n" +
                    "AWAITING INPUT"
                );
                break;
        }
    }

    private void SetDisplay(string message)
    {
        if (displayText == null)
        {
            return;
        }

        displayText.text = message;
        displayText.color = displayTextColour;
    }

    private static void SetObjectActive(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

    private void OnDisable()
    {
        if (alarmSource != null && alarmVolumeLowered)
        {
            alarmSource.volume = originalAlarmVolume;
        }
    }
}