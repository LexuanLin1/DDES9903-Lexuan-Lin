using System.Collections;
using TMPro;
using UnityEngine;

public sealed class FamilyMemoryTrigger : MonoBehaviour
{
    [Header("Player Voice")]
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioClip familyPromiseVoice;

    [Header("Subtitle")]
    [SerializeField] private CanvasGroup subtitleGroup;
    [SerializeField] private TMP_Text subtitleText;

    [TextArea(2, 4)]
    [SerializeField]
    private string subtitleMessage =
        "Ten years... I promised I would come home.\n" +
        "Whatever it takes, I have to see them again.";

    [SerializeField, Min(0f)]
    private float subtitleFadeDuration = 0.3f;

    [Header("Attention Light")]
    [SerializeField] private Light attentionLight;

    [SerializeField, Min(0f)]
    private float lightFadeDuration = 1.2f;

    [Header("Cabinet Movement")]
    [SerializeField] private Transform cabinetToMove;
    [SerializeField] private Transform cabinetOpenPoint;

    [SerializeField, Min(0.1f)]
    private float cabinetMoveDuration = 2.2f;

    [SerializeField, Min(0f)]
    private float delayBeforeCabinetMoves = 0.35f;

    [SerializeField] private GameObject cockpitPathBlocker;

    [Header("Cabinet Audio")]
    [SerializeField] private AudioSource cabinetMoveAudioSource;
    [SerializeField] private AudioClip cabinetMoveSound;

    [Header("Fallback")]
    [SerializeField, Min(0f)]
    private float fallbackVoiceDuration = 6f;

    private bool hasTriggered;

    private void Awake()
    {
        hasTriggered = false;

        if (subtitleGroup != null)
        {
            subtitleGroup.alpha = 0f;
            subtitleGroup.interactable = false;
            subtitleGroup.blocksRaycasts = false;
        }

        if (cockpitPathBlocker != null)
        {
            cockpitPathBlocker.SetActive(true);
        }
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
        StartCoroutine(PlayFamilySequence());
    }

    private IEnumerator PlayFamilySequence()
    {
        if (subtitleText != null)
        {
            subtitleText.text = subtitleMessage;
        }

        yield return StartCoroutine(
            FadeSubtitle(
                0f,
                1f,
                subtitleFadeDuration
            )
        );

        float voiceDuration = fallbackVoiceDuration;

        if (voiceSource != null &&
            familyPromiseVoice != null)
        {
            voiceSource.Stop();
            voiceSource.clip = familyPromiseVoice;
            voiceSource.loop = false;
            voiceSource.Play();

            voiceDuration = familyPromiseVoice.length;
        }

        yield return new WaitForSeconds(voiceDuration);

        yield return StartCoroutine(
            FadeSubtitle(
                1f,
                0f,
                subtitleFadeDuration
            )
        );

        yield return new WaitForSeconds(
            delayBeforeCabinetMoves
        );

        if (cabinetMoveAudioSource != null &&
            cabinetMoveSound != null)
        {
            cabinetMoveAudioSource.PlayOneShot(
                cabinetMoveSound
            );
        }

        Coroutine lightRoutine = null;

        if (attentionLight != null)
        {
            lightRoutine = StartCoroutine(
                FadeAttentionLight()
            );
        }

        yield return StartCoroutine(
            MoveCabinet()
        );

        if (lightRoutine != null)
        {
            yield return lightRoutine;
        }

        if (cockpitPathBlocker != null)
        {
            cockpitPathBlocker.SetActive(false);
        }

        Debug.Log(
            "Family memory completed. Cockpit path opened.",
            this
        );
    }

    private IEnumerator MoveCabinet()
    {
        if (cabinetToMove == null ||
            cabinetOpenPoint == null)
        {
            Debug.LogWarning(
                "Cabinet movement references are missing.",
                this
            );

            yield break;
        }

        Vector3 startPosition =
            cabinetToMove.position;

        Quaternion startRotation =
            cabinetToMove.rotation;

        Vector3 targetPosition =
            cabinetOpenPoint.position;

        Quaternion targetRotation =
            cabinetOpenPoint.rotation;

        float elapsedTime = 0f;

        while (elapsedTime < cabinetMoveDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsedTime / cabinetMoveDuration
                );

            float smoothProgress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            cabinetToMove.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    smoothProgress
                );

            cabinetToMove.rotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    smoothProgress
                );

            yield return null;
        }

        cabinetToMove.SetPositionAndRotation(
            targetPosition,
            targetRotation
        );
    }

    private IEnumerator FadeSubtitle(
        float startAlpha,
        float endAlpha,
        float duration)
    {
        if (subtitleGroup == null)
        {
            yield break;
        }

        if (duration <= 0f)
        {
            subtitleGroup.alpha = endAlpha;
            yield break;
        }

        float elapsedTime = 0f;
        subtitleGroup.alpha = startAlpha;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsedTime / duration
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

    private IEnumerator FadeAttentionLight()
    {
        if (attentionLight == null)
        {
            yield break;
        }

        float startIntensity =
            attentionLight.intensity;

        if (lightFadeDuration <= 0f)
        {
            attentionLight.intensity = 0f;
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < lightFadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsedTime / lightFadeDuration
                );

            attentionLight.intensity =
                Mathf.Lerp(
                    startIntensity,
                    0f,
                    progress
                );

            yield return null;
        }

        attentionLight.intensity = 0f;
    }
}