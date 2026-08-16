using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class A2EndingController : MonoBehaviour
{
    [Header("Return To Earth Route")]
    [SerializeField] private GameObject escapePodRestriction;
    [SerializeField] private GameObject escapePodBoardInteraction;

    [Header("MILO")]
    [SerializeField] private GameObject miloActive;
    [SerializeField] private GameObject miloMaintenancePod;
    [SerializeField] private Transform miloEscapePodPoint;

    [Header("Ending Display")]
    [SerializeField] private Image endingImage;
    [SerializeField] private Sprite returnEndingImage;
    [SerializeField] private Sprite stayEndingImage;
    [SerializeField] private TMP_Text endingTitleText;
    [SerializeField] private TMP_Text endingBodyText;

    [Header("Fade To Black")]
    [SerializeField] private CanvasGroup endingFadeGroup;

    [SerializeField, Min(0.1f)]
    private float fadeToBlackDuration = 1.2f;

    [SerializeField, Min(0f)]
    private float blackHoldDuration = 0.5f;

    [SerializeField, Min(0.1f)]
    private float fadeFromBlackDuration = 1.0f;

    [Header("Voice Sources")]
    [SerializeField] private AudioSource systemVoiceSource;
    [SerializeField] private AudioSource captainVoiceSource;
    [SerializeField] private AudioSource miloVoiceSource;

    [Header("Ending Epilogue Voice Source")]
    [SerializeField] private AudioSource endingVoiceSource;

    [Header("Return Ending Voices")]
    [SerializeField] private AudioClip escapeLaunchVoice;
    [SerializeField] private AudioClip miloReturnVoice;
    [SerializeField] private AudioClip captainReturnVoice;

    [Header("Stay Ending Voices")]
    [SerializeField] private AudioClip miloDepartureVoice;
    [SerializeField] private AudioClip captainMiloDepartureVoice;
    [SerializeField] private AudioClip staySystemVoice;
    [SerializeField] private AudioClip captainStayVoice;

    [Header("Final Epilogue Voices")]
    [SerializeField] private AudioClip daughterHomeVoice;
    [SerializeField] private AudioClip rescueFoundVoice;

    [Header("Shared Subtitles")]
    [SerializeField] private CanvasGroup subtitleGroup;
    [SerializeField] private TMP_Text subtitleText;

    [SerializeField]
    private Color systemSubtitleColour =
        new Color(1f, 0.75f, 0.75f, 1f);

    [SerializeField]
    private Color captainSubtitleColour =
        Color.white;

    [SerializeField]
    private Color miloSubtitleColour =
        new Color(0.65f, 0.9f, 1f, 1f);

    [TextArea(2, 4)]
    [SerializeField]
    private string escapeLaunchSubtitle =
        "Escape pod sealed.\n" +
        "Launch sequence initiated.";

    [TextArea(2, 4)]
    [SerializeField]
    private string miloReturnSubtitle =
        "Ready, Captain.";

    [TextArea(2, 4)]
    [SerializeField]
    private string captainReturnSubtitle =
        "Then let's go home.";

    [TextArea(2, 4)]
    [SerializeField]
    private string miloDepartureSubtitle =
        "I'll take the maintenance pod\n" +
        "and send the distress signal.";

    [TextArea(2, 4)]
    [SerializeField]
    private string captainMiloDepartureSubtitle =
        "Go, MILO.\n" +
        "Tell them we're still here.";

    [TextArea(2, 5)]
    [SerializeField]
    private string staySystemSubtitle =
        "Emergency transmission active.\n" +
        "Remaining power redirected to navigation\n" +
        "and life support.";

    [TextArea(2, 4)]
    [SerializeField]
    private string captainStaySubtitle =
        "Then we stay...\n" +
        "and make sure their story survives.";

    [Header("Ending Text")]
    [TextArea(2, 5)]
    [SerializeField]
    private string returnEndingBody =
        "After ten years, the Captain finally returns home.";

    [TextArea(2, 5)]
    [SerializeField]
    private string stayEndingBody =
        "The ship continues toward Earth orbit,\n" +
        "carrying the crew's legacy and one last chance to be found.";

    [Header("Timing")]
    [SerializeField, Min(0f)]
    private float dialogueGap = 0.5f;

    [SerializeField, Min(0f)]
    private float miloDepartureDelay = 0.8f;

    [SerializeField, Min(0f)]
    private float preFadeDelay = 0.8f;

    [SerializeField, Min(0f)]
    private float endingRevealDelay = 0.4f;

    [SerializeField, Min(0f)]
    private float subtitleFadeDuration = 0.2f;

    [SerializeField, Min(0f)]
    private float subtitleExtraTime = 0.2f;

    [SerializeField, Min(0.5f)]
    private float fallbackVoiceDuration = 3f;

    private bool returnRouteUnlocked;
    private bool endingRunning;
    private bool endingComplete;

    private void Awake()
    {
        if (escapePodBoardInteraction != null)
        {
            escapePodBoardInteraction.SetActive(false);
        }

        HideEndingDisplay();

        if (subtitleGroup != null)
        {
            subtitleGroup.alpha = 0f;
        }

        if (endingFadeGroup != null)
        {
            endingFadeGroup.alpha = 0f;
            endingFadeGroup.interactable = false;
            endingFadeGroup.blocksRaycasts = false;
        }
    }

    public void BeginReturnRoute()
    {
        if (returnRouteUnlocked ||
            endingRunning ||
            endingComplete)
        {
            return;
        }

        returnRouteUnlocked = true;

        if (escapePodRestriction != null)
        {
            escapePodRestriction.SetActive(false);
        }

        if (escapePodBoardInteraction != null)
        {
            escapePodBoardInteraction.SetActive(true);
        }

        MoveMiloToEscapePod();

        Debug.Log(
            "A2 ENDING: Return route unlocked.",
            this
        );
    }

    public void BoardEscapePod()
    {
        if (!returnRouteUnlocked ||
            endingRunning ||
            endingComplete)
        {
            return;
        }

        endingRunning = true;

        if (escapePodBoardInteraction != null)
        {
            escapePodBoardInteraction.SetActive(false);
        }

        StartCoroutine(
            ReturnEndingSequence()
        );
    }

    public void BeginStayEnding()
    {
        if (endingRunning ||
            endingComplete)
        {
            return;
        }

        endingRunning = true;

        StartCoroutine(
            StayEndingSequence()
        );
    }

    private IEnumerator ReturnEndingSequence()
    {
        yield return StartCoroutine(
            PlayDialogue(
                systemVoiceSource,
                escapeLaunchVoice,
                escapeLaunchSubtitle,
                systemSubtitleColour
            )
        );

        if (IsMiloRepaired())
        {
            yield return new WaitForSeconds(
                dialogueGap
            );

            yield return StartCoroutine(
                PlayDialogue(
                    miloVoiceSource,
                    miloReturnVoice,
                    miloReturnSubtitle,
                    miloSubtitleColour
                )
            );
        }

        yield return new WaitForSeconds(
            dialogueGap
        );

        yield return StartCoroutine(
            PlayDialogue(
                captainVoiceSource,
                captainReturnVoice,
                captainReturnSubtitle,
                captainSubtitleColour
            )
        );

        yield return new WaitForSeconds(
            preFadeDelay
        );

        // Fade away from the spaceship.
        yield return StartCoroutine(
            FadeBlack(
                0f,
                1f,
                fadeToBlackDuration
            )
        );

        yield return new WaitForSeconds(
            blackHoldDuration
        );

        // Daughter speaks while the screen is black.
        yield return StartCoroutine(
            PlayEndingVoiceAndWait(
                daughterHomeVoice
            )
        );

        // Prepare the final image while still black.
        ShowReturnEnding();

        yield return new WaitForSeconds(
            endingRevealDelay
        );

        // Reveal the ending screen.
        yield return StartCoroutine(
            FadeBlack(
                1f,
                0f,
                fadeFromBlackDuration
            )
        );

        endingRunning = false;
        endingComplete = true;

        Debug.Log(
            "A2 ENDING COMPLETE: RETURN TO EARTH.",
            this
        );
    }

    private IEnumerator StayEndingSequence()
    {
        bool miloRepaired =
            IsMiloRepaired();

        if (miloRepaired)
        {
            yield return StartCoroutine(
                PlayDialogue(
                    miloVoiceSource,
                    miloDepartureVoice,
                    miloDepartureSubtitle,
                    miloSubtitleColour
                )
            );

            yield return new WaitForSeconds(
                dialogueGap
            );

            yield return StartCoroutine(
                PlayDialogue(
                    captainVoiceSource,
                    captainMiloDepartureVoice,
                    captainMiloDepartureSubtitle,
                    captainSubtitleColour
                )
            );

            yield return new WaitForSeconds(
                miloDepartureDelay
            );

            DepartMilo();
        }

        yield return new WaitForSeconds(
            dialogueGap
        );

        yield return StartCoroutine(
            PlayDialogue(
                systemVoiceSource,
                staySystemVoice,
                staySystemSubtitle,
                systemSubtitleColour
            )
        );

        yield return new WaitForSeconds(
            dialogueGap
        );

        yield return StartCoroutine(
            PlayDialogue(
                captainVoiceSource,
                captainStayVoice,
                captainStaySubtitle,
                captainSubtitleColour
            )
        );

        yield return new WaitForSeconds(
            preFadeDelay
        );

        // Fade away from the bridge.
        yield return StartCoroutine(
            FadeBlack(
                0f,
                1f,
                fadeToBlackDuration
            )
        );

        yield return new WaitForSeconds(
            blackHoldDuration
        );

        // MILO's earlier repair now has a final consequence.
        if (miloRepaired)
        {
            yield return StartCoroutine(
                PlayEndingVoiceAndWait(
                    rescueFoundVoice
                )
            );
        }

        // Prepare ending image while the player sees black.
        ShowStayEnding();

        yield return new WaitForSeconds(
            endingRevealDelay
        );

        // Reveal the ending.
        yield return StartCoroutine(
            FadeBlack(
                1f,
                0f,
                fadeFromBlackDuration
            )
        );

        endingRunning = false;
        endingComplete = true;

        Debug.Log(
            "A2 ENDING COMPLETE: STAY WITH THE SHIP.",
            this
        );
    }

    private IEnumerator PlayEndingVoiceAndWait(
        AudioClip clip)
    {
        if (clip == null)
        {
            yield return new WaitForSeconds(
                fallbackVoiceDuration
            );

            yield break;
        }

        if (endingVoiceSource == null)
        {
            yield return new WaitForSeconds(
                clip.length
            );

            yield break;
        }

        endingVoiceSource.Stop();
        endingVoiceSource.clip = clip;
        endingVoiceSource.loop = false;
        endingVoiceSource.Play();

        yield return new WaitForSeconds(
            clip.length
        );
    }

    private IEnumerator FadeBlack(
        float startAlpha,
        float endAlpha,
        float duration)
    {
        if (endingFadeGroup == null)
        {
            yield break;
        }

        if (duration <= 0f)
        {
            endingFadeGroup.alpha =
                endAlpha;

            yield break;
        }

        float elapsedTime = 0f;

        endingFadeGroup.alpha =
            startAlpha;

        while (elapsedTime < duration)
        {
            elapsedTime +=
                Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsedTime /
                    duration
                );

            endingFadeGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    endAlpha,
                    progress
                );

            yield return null;
        }

        endingFadeGroup.alpha =
            endAlpha;
    }

    private void MoveMiloToEscapePod()
    {
        if (!IsMiloRepaired())
        {
            return;
        }

        if (miloActive == null ||
            miloEscapePodPoint == null)
        {
            return;
        }

        miloActive.transform.SetParent(
            null,
            true
        );

        miloActive.transform.SetPositionAndRotation(
            miloEscapePodPoint.position,
            miloEscapePodPoint.rotation
        );

        if (miloMaintenancePod != null)
        {
            miloMaintenancePod.SetActive(false);
        }
    }

    private void DepartMilo()
    {
        if (miloActive != null)
        {
            miloActive.SetActive(false);
        }

        if (miloMaintenancePod != null)
        {
            miloMaintenancePod.SetActive(false);
        }

        Debug.Log(
            "A2: MILO departed in the maintenance emergency pod.",
            this
        );
    }

    private bool IsMiloRepaired()
    {
        return
            A2NarrativeStateManager.Instance != null &&
            A2NarrativeStateManager.Instance.MiloRepaired;
    }

    private void ShowReturnEnding()
    {
        if (endingImage != null)
        {
            endingImage.sprite =
                returnEndingImage;

            endingImage.enabled =
                returnEndingImage != null;
        }

        if (endingTitleText != null)
        {
            endingTitleText.text =
                "RETURN TO EARTH";
        }

        if (endingBodyText != null)
        {
            endingBodyText.text =
                returnEndingBody;
        }
    }

    private void ShowStayEnding()
    {
        if (endingImage != null)
        {
            endingImage.sprite =
                stayEndingImage;

            endingImage.enabled =
                stayEndingImage != null;
        }

        if (endingTitleText != null)
        {
            endingTitleText.text =
                "STAY WITH THE SHIP";
        }

        if (endingBodyText != null)
        {
            endingBodyText.text =
                stayEndingBody;
        }
    }

    private void HideEndingDisplay()
    {
        if (endingTitleText != null)
        {
            endingTitleText.text =
                string.Empty;
        }

        if (endingBodyText != null)
        {
            endingBodyText.text =
                string.Empty;
        }
    }

    private IEnumerator PlayDialogue(
        AudioSource source,
        AudioClip clip,
        string message,
        Color subtitleColour)
    {
        while (IsAnyVoicePlaying())
        {
            yield return null;
        }

        if (subtitleText != null)
        {
            subtitleText.text =
                message;

            subtitleText.color =
                subtitleColour;
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
            clip != null &&
            clip.length > 0f
                ? clip.length
                : fallbackVoiceDuration;

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

    private bool IsAnyVoicePlaying()
    {
        return
            IsPlaying(systemVoiceSource) ||
            IsPlaying(captainVoiceSource) ||
            IsPlaying(miloVoiceSource) ||
            IsPlaying(endingVoiceSource);
    }

    private static bool IsPlaying(
        AudioSource source)
    {
        return
            source != null &&
            source.isPlaying;
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
}