using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public sealed class PostTeleportNarrativeSequence : MonoBehaviour
{
    [Header("System Voice")]
    [FormerlySerializedAs("securityVoiceSource")]
    [SerializeField] private AudioSource systemVoiceSource;

    [FormerlySerializedAs("securityVoiceClip")]
    [SerializeField] private AudioClip systemVoiceClip;

    [FormerlySerializedAs("securitySubtitle")]
    [TextArea(3, 7)]
    [SerializeField]
    private string systemSubtitle =
        "Escape pod launch authorization incomplete.\n" +
        "Main power is nearly exhausted.\n" +
        "Orbital stability is failing.\n" +
        "Captain, proceed to the bridge and make your final decision.";

    [Header("Captain Voice")]
    [FormerlySerializedAs("playerVoiceSource")]
    [SerializeField] private AudioSource captainVoiceSource;

    [FormerlySerializedAs("playerVoiceClip")]
    [SerializeField] private AudioClip captainVoiceClip;

    [FormerlySerializedAs("playerSubtitle")]
    [TextArea(2, 5)]
    [SerializeField]
    private string captainSubtitle =
        "The bridge... right.\n" +
        "I need to decide what happens to the last of our power.";

    [Header("Shared Subtitles")]
    [SerializeField] private CanvasGroup subtitleGroup;
    [SerializeField] private TMP_Text subtitleText;

    [FormerlySerializedAs("securitySubtitleColour")]
    [SerializeField]
    private Color systemSubtitleColour =
        new Color(1f, 0.65f, 0.65f, 1f);

    [FormerlySerializedAs("playerSubtitleColour")]
    [SerializeField]
    private Color captainSubtitleColour =
        Color.white;

    [Header("Alarm")]
    [SerializeField] private AudioSource alarmSource;

    [SerializeField, Range(0f, 1f)]
    private float alarmDialogueVolume = 0.12f;

    [Header("Timing")]
    [SerializeField, Min(0f)]
    private float delayAfterTeleport = 0.6f;

    [SerializeField, Min(0f)]
    private float gapBetweenVoices = 0.8f;

    [SerializeField, Min(0f)]
    private float subtitleFadeDuration = 0.2f;

    [SerializeField, Min(0f)]
    private float subtitleExtraTime = 0.2f;

    [SerializeField, Min(0.5f)]
    private float fallbackVoiceDuration = 4f;

    private bool hasPlayed;
    private float originalAlarmVolume;

    public void PlayOnce()
    {
        if (hasPlayed)
        {
            return;
        }

        hasPlayed = true;

        StartCoroutine(
            RunSequence()
        );
    }

    private IEnumerator RunSequence()
    {
        LowerAlarmVolume();

        yield return new WaitForSeconds(
            delayAfterTeleport
        );

        // Ship system warning.
        yield return StartCoroutine(
            PlayDialogue(
                systemVoiceSource,
                systemVoiceClip,
                systemSubtitle,
                systemSubtitleColour
            )
        );

        yield return new WaitForSeconds(
            gapBetweenVoices
        );

        // Captain reflects on the final decision.
        yield return StartCoroutine(
            PlayDialogue(
                captainVoiceSource,
                captainVoiceClip,
                captainSubtitle,
                captainSubtitleColour
            )
        );

        RestoreAlarmVolume();

        Debug.Log(
            "A2: Escape pod rejection narrative completed.",
            this
        );
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

        yield return StartCoroutine(
            FadeSubtitle(0f, 1f)
        );

        if (voiceSource != null &&
            voiceClip != null)
        {
            voiceSource.Stop();
            voiceSource.clip = voiceClip;
            voiceSource.loop = false;
            voiceSource.Play();
        }

        float voiceDuration =
            voiceClip != null &&
            voiceClip.length > 0f
                ? voiceClip.length
                : fallbackVoiceDuration;

        yield return new WaitForSeconds(
            voiceDuration + subtitleExtraTime
        );

        yield return StartCoroutine(
            FadeSubtitle(1f, 0f)
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
            subtitleGroup.alpha = endAlpha;
            yield break;
        }

        float elapsedTime = 0f;

        subtitleGroup.alpha =
            startAlpha;

        while (elapsedTime <
               subtitleFadeDuration)
        {
            elapsedTime += Time.deltaTime;

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

    private void LowerAlarmVolume()
    {
        if (alarmSource == null)
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
    }

    private void RestoreAlarmVolume()
    {
        if (alarmSource != null)
        {
            alarmSource.volume =
                originalAlarmVolume;
        }
    }

    private void OnDisable()
    {
        RestoreAlarmVolume();
    }
}