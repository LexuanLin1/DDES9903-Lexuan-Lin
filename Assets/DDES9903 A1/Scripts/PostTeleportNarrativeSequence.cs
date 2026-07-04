using System.Collections;
using TMPro;
using UnityEngine;

public sealed class PostTeleportNarrativeSequence : MonoBehaviour
{
    [Header("Security Voice")]
    [SerializeField] private AudioSource securityVoiceSource;
    [SerializeField] private AudioClip securityVoiceClip;

    [TextArea(2, 5)]
    [SerializeField]
    private string securitySubtitle =
        "Unauthorized biological presence confirmed.\n" +
        "Crew authorization records have expired.\n" +
        "Security containment remains active.\n" +
        "Manual identity verification required.";

    [Header("Player Voice")]
    [SerializeField] private AudioSource playerVoiceSource;
    [SerializeField] private AudioClip playerVoiceClip;

    [TextArea(2, 5)]
    [SerializeField]
    private string playerSubtitle =
        "The observation deck is behind the elevator.\n" +
        "If the old verification terminal still works,\n" +
        "I can restore my crew authorization there.";

    [Header("Shared Subtitles")]
    [SerializeField] private CanvasGroup subtitleGroup;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField]
    private Color securitySubtitleColour =
        new Color(1f, 0.65f, 0.65f, 1f);
    [SerializeField] private Color playerSubtitleColour = Color.white;

    [Header("Alarm")]
    [SerializeField] private AudioSource alarmSource;
    [SerializeField, Range(0f, 1f)] private float alarmDialogueVolume = 0.12f;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float delayAfterTeleport = 0.6f;
    [SerializeField, Min(0f)] private float gapBetweenVoices = 0.4f;
    [SerializeField, Min(0f)] private float subtitleFadeDuration = 0.2f;
    [SerializeField, Min(0f)] private float subtitleExtraTime = 0.2f;
    [SerializeField, Min(0.5f)] private float fallbackVoiceDuration = 4f;

    private bool hasPlayed;
    private float originalAlarmVolume;

    public void PlayOnce()
    {
        if (hasPlayed)
        {
            return;
        }

        hasPlayed = true;
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        LowerAlarmVolume();

        yield return new WaitForSeconds(delayAfterTeleport);

        yield return StartCoroutine(
            PlayDialogue(
                securityVoiceSource,
                securityVoiceClip,
                securitySubtitle,
                securitySubtitleColour
            )
        );

        yield return new WaitForSeconds(gapBetweenVoices);

        yield return StartCoroutine(
            PlayDialogue(
                playerVoiceSource,
                playerVoiceClip,
                playerSubtitle,
                playerSubtitleColour
            )
        );

        RestoreAlarmVolume();
    }

    private IEnumerator PlayDialogue(
        AudioSource voiceSource,
        AudioClip voiceClip,
        string message,
        Color subtitleColour)
    {
        if (subtitleText != null)
        {
            subtitleText.text = message;
            subtitleText.color = subtitleColour;
        }

        yield return StartCoroutine(FadeSubtitle(0f, 1f));

        if (voiceSource != null && voiceClip != null)
        {
            voiceSource.Stop();
            voiceSource.clip = voiceClip;
            voiceSource.loop = false;
            voiceSource.Play();
        }

        float voiceDuration =
            voiceClip != null && voiceClip.length > 0f
                ? voiceClip.length
                : fallbackVoiceDuration;

        yield return new WaitForSeconds(
            voiceDuration + subtitleExtraTime
        );

        yield return StartCoroutine(FadeSubtitle(1f, 0f));
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
            subtitleGroup.alpha = endAlpha;
            yield break;
        }

        float elapsedTime = 0f;
        subtitleGroup.alpha = startAlpha;

        while (elapsedTime < subtitleFadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime / subtitleFadeDuration
            );

            subtitleGroup.alpha = Mathf.Lerp(
                startAlpha,
                endAlpha,
                progress
            );

            yield return null;
        }

        subtitleGroup.alpha = endAlpha;
    }

    private void LowerAlarmVolume()
    {
        if (alarmSource == null)
        {
            return;
        }

        originalAlarmVolume = alarmSource.volume;
        alarmSource.volume = Mathf.Min(
            originalAlarmVolume,
            alarmDialogueVolume
        );
    }

    private void RestoreAlarmVolume()
    {
        if (alarmSource != null)
        {
            alarmSource.volume = originalAlarmVolume;
        }
    }

    private void OnDisable()
    {
        RestoreAlarmVolume();
    }
}
