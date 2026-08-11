using System.Collections;
using TMPro;
using UnityEngine;

public sealed class CockpitMissionSequence : MonoBehaviour
{
    [Header("Mission Display")]
    [SerializeField] private TMP_Text missionText;
    [SerializeField] private Color missionTextColour = Color.black;

    [Header("Briefing Button")]
    [SerializeField] private GameObject briefingButton;

    [Header("AI Voice")]
    [SerializeField] private AudioSource aiVoiceSource;
    [SerializeField] private AudioClip navigationVoice;
    [SerializeField] private AudioClip routeDeviationVoice;
    [SerializeField] private AudioClip cryoReserveVoice;
    [SerializeField] private AudioClip energyObjectiveVoice;
    [SerializeField] private AudioClip securityWarningVoice;

    [Header("Player Response")]
    [SerializeField] private AudioSource playerVoiceSource;
    [SerializeField] private AudioClip playerObjectiveVoice;

    [SerializeField] private CanvasGroup subtitleGroup;
    [SerializeField] private TMP_Text subtitleText;

    [TextArea(2, 4)]
    [SerializeField]
    private string playerObjectiveSubtitle =
        "One crystal... That's my way home.\n" +
        "I have to bring it back.";

    [Header("Mission Progress")]
    [SerializeField] private GameObject[] objectsToEnableAfterBriefing;
    [SerializeField] private GameObject[] objectsToDisableAfterBriefing;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float messageGap = 0.35f;
    [SerializeField, Min(0f)] private float subtitleFadeDuration = 0.25f;
    [SerializeField, Min(0f)] private float subtitleExtraTime = 0.3f;
    [SerializeField, Min(0.5f)] private float fallbackClipDuration = 4f;

    private bool briefingStarted;

    private void Awake()
    {
        briefingStarted = false;

        if (briefingButton != null)
        {
            briefingButton.SetActive(true);
        }

        if (subtitleGroup != null)
        {
            subtitleGroup.alpha = 0f;
            subtitleGroup.interactable = false;
            subtitleGroup.blocksRaycasts = false;
        }

        SetObjectsActive(
            objectsToEnableAfterBriefing,
            false
        );

        SetObjectsActive(
            objectsToDisableAfterBriefing,
            true
        );

        SetMissionText(
            "NAVIGATION TERMINAL\n\n" +
            "ACCESS REQUIRED"
        );
    }

    public void BeginBriefing()
    {
        if (briefingStarted)
        {
            return;
        }

        briefingStarted = true;

        if (briefingButton != null)
        {
            briefingButton.SetActive(false);
        }

        StartCoroutine(RunBriefing());
    }

    private IEnumerator RunBriefing()
    {
        yield return StartCoroutine(
            ShowMissionMessage(
                "NAVIGATION CORE ONLINE\n\n" +
                "RETURN ROUTE: ACTIVE\n\n" +
                "HOME BEACON: LOCKED",
                navigationVoice,
                4f
            )
        );

        yield return StartCoroutine(
            ShowMissionMessage(
                "ROUTE DEVIATION CONFIRMED\n\n" +
                "EXPECTED JOURNEY: 5 YEARS\n\n" +
                "ACTUAL ELAPSED TIME: 10 YEARS",
                routeDeviationVoice,
                7f
            )
        );

        yield return StartCoroutine(
            ShowMissionMessage(
                "CRYOSLEEP RESERVE: CRITICAL\n\n" +
                "FINAL APPROACH: UNSAFE",
                cryoReserveVoice,
                5f
            )
        );

        yield return StartCoroutine(
            ShowMissionMessage(
                "EMERGENCY POWER SOURCE DETECTED\n\n" +
                "LOCATION: UPPER DECK SECURITY VAULT",
                energyObjectiveVoice,
                7f
            )
        );

        yield return StartCoroutine(
            ShowMissionMessage(
                "SECURITY WARNING\n\n" +
                "CREW AUTHORIZATION: EXPIRED\n\n" +
                "MANUAL VERIFICATION MAY BE REQUIRED",
                securityWarningVoice,
                7f
            )
        );

        yield return StartCoroutine(
            PlayPlayerResponse()
        );

        SetMissionText(
            "PRIMARY OBJECTIVE\n\n" +
            "RETRIEVE EMERGENCY ENERGY CRYSTAL\n\n" +
            "RETURN TO CRYOSLEEP CHAMBER"
        );

        SetObjectsActive(
            objectsToEnableAfterBriefing,
            true
        );

        SetObjectsActive(
            objectsToDisableAfterBriefing,
            false
        );

        Debug.Log(
            "Cockpit mission briefing completed.",
            this
        );
    }

    private IEnumerator ShowMissionMessage(
        string message,
        AudioClip voiceClip,
        float fallbackDuration)
    {
        SetMissionText(message);
        PlayAudio(aiVoiceSource, voiceClip);

        yield return new WaitForSeconds(
            GetClipDuration(
                voiceClip,
                fallbackDuration
            )
        );

        yield return new WaitForSeconds(messageGap);
    }

    private IEnumerator PlayPlayerResponse()
    {
        if (subtitleText != null)
        {
            subtitleText.text =
                playerObjectiveSubtitle;
        }

        yield return StartCoroutine(
            FadeSubtitle(0f, 1f)
        );

        PlayAudio(
            playerVoiceSource,
            playerObjectiveVoice
        );

        yield return new WaitForSeconds(
            GetClipDuration(
                playerObjectiveVoice,
                5f
            ) + subtitleExtraTime
        );

        yield return StartCoroutine(
            FadeSubtitle(1f, 0f)
        );

        yield return new WaitForSeconds(messageGap);
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

    private void SetMissionText(string message)
    {
        if (missionText == null)
        {
            return;
        }

        missionText.text = message;
        missionText.color = missionTextColour;
    }

    private void PlayAudio(
        AudioSource source,
        AudioClip clip)
    {
        if (source == null || clip == null)
        {
            return;
        }

        source.Stop();
        source.clip = clip;
        source.loop = false;
        source.Play();
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

    private static void SetObjectsActive(
        GameObject[] targets,
        bool active)
    {
        if (targets == null)
        {
            return;
        }

        foreach (GameObject target in targets)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
