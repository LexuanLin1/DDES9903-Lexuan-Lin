using System.Collections;
using TMPro;
using UnityEngine;

public sealed class OneShotPlayerDialogueTrigger : MonoBehaviour
{
    [Header("Voice")]
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioClip voiceClip;

    [Header("Subtitle")]
    [SerializeField] private CanvasGroup subtitleGroup;
    [SerializeField] private TMP_Text subtitleText;

    [TextArea(2, 5)]
    [SerializeField]
    private string subtitleMessage =
        "These pods... they're both offline.\n" +
        "What happened on this ship during those ten years?";

    [Header("Timing")]
    [SerializeField, Min(0f)]
    private float subtitleFadeDuration = 0.25f;

    [SerializeField, Min(0f)]
    private float subtitleExtraTime = 0.35f;

    [SerializeField, Min(0.5f)]
    private float fallbackVoiceDuration = 6f;

    private bool hasTriggered;

    private void Awake()
    {
        hasTriggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
        {
            return;
        }

        CharacterController player =
            other.GetComponentInParent<CharacterController>();

        if (player == null)
        {
            Transform root = other.transform.root;

            player = root.GetComponentInChildren<CharacterController>(
                true
            );
        }

        if (player == null)
        {
            return;
        }

        hasTriggered = true;
        StartCoroutine(PlayDialogue());
    }

    private IEnumerator PlayDialogue()
    {
        if (subtitleText != null)
        {
            subtitleText.text = subtitleMessage;
        }

        yield return StartCoroutine(
            FadeSubtitle(0f, 1f)
        );

        float voiceDuration = fallbackVoiceDuration;

        if (voiceSource != null &&
            voiceClip != null)
        {
            voiceSource.Stop();
            voiceSource.clip = voiceClip;
            voiceSource.loop = false;
            voiceSource.Play();

            voiceDuration = voiceClip.length;
        }

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
        subtitleGroup.alpha = startAlpha;

        while (elapsedTime < subtitleFadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsedTime / subtitleFadeDuration
                );

            subtitleGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    endAlpha,
                    progress
                );

            yield return null;
        }

        subtitleGroup.alpha = endAlpha;
    }
}