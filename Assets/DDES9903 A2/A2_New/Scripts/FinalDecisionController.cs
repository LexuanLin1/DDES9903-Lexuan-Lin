using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class FinalDecisionController : MonoBehaviour
{
    [Header("Decision Screen")]
    [SerializeField] private Image decisionImage;
    [SerializeField] private Sprite returnToEarthImage;
    [SerializeField] private Sprite stayWithShipImage;
    [SerializeField] private Sprite finalSummaryImage;

    [Header("Choice Interactions")]
    [SerializeField] private GameObject returnToEarthButton;
    [SerializeField] private GameObject stayWithShipButton;

    [Header("MILO")]
    [SerializeField] private GameObject miloMaintenancePod;

    [Header("Voice Sources")]
    [SerializeField] private AudioSource systemVoiceSource;
    [SerializeField] private AudioSource miloVoiceSource;

    [Header("Opening Voice")]
    [SerializeField] private AudioClip miloReadyVoice;
    [SerializeField] private AudioClip systemIntroVoice;

    [Header("Option Voices")]
    [SerializeField] private AudioClip returnToEarthVoice;
    [SerializeField] private AudioClip stayWithShipVoice;

    [Header("MILO Rescue Voice")]
    [SerializeField] private AudioClip miloRescueSystemVoice;

    [Header("Final Decision Voice")]
    [SerializeField] private AudioClip finalDecisionVoice;

    [Header("Choice Result Voices")]
    [SerializeField] private AudioClip noEnergyCoreVoice;
    [SerializeField] private AudioClip returnConfirmedVoice;
    [SerializeField] private AudioClip stayConfirmedVoice;

    [Header("Shared Subtitles")]
    [SerializeField] private CanvasGroup subtitleGroup;
    [SerializeField] private TMP_Text subtitleText;

    [SerializeField]
    private Color systemSubtitleColour =
        new Color(1f, 0.75f, 0.75f, 1f);

    [SerializeField]
    private Color miloSubtitleColour =
        new Color(0.65f, 0.9f, 1f, 1f);

    [TextArea(2, 4)]
    [SerializeField]
    private string miloReadySubtitle =
        "Captain... whatever you decide,\n" +
        "I'm ready.";

    [TextArea(3, 6)]
    [SerializeField]
    private string systemIntroSubtitle =
        "Captain. Main power is nearly exhausted.\n" +
        "The final escape pod is ready.\n" +
        "Launching it will require the remaining energy of the Energy Core.";

    [TextArea(3, 7)]
    [SerializeField]
    private string returnToEarthSubtitle =
        "If you launch the escape pod, this vessel will lose its remaining power.\n" +
        "You will have a chance to return to Earth and reunite with the family\n" +
        "who has waited for you for ten years.\n" +
        "The ship, and everything left aboard, may never return.";

    [TextArea(3, 7)]
    [SerializeField]
    private string stayWithShipSubtitle =
        "If you leave the Energy Core aboard, limited power may remain available\n" +
        "for navigation and emergency transmission.\n" +
        "There is a small possibility that the ship may reach an Earth orbital station,\n" +
        "preserving your crew's legacy and the story of this mission.";

    [TextArea(3, 6)]
    [SerializeField]
    private string miloRescueSubtitle =
        "MILO-01 can use the maintenance emergency pod to return ahead\n" +
        "and transmit a distress signal.\n" +
        "If you remain aboard, this will increase the possibility of rescue.";

    [TextArea(2, 4)]
    [SerializeField]
    private string finalDecisionSubtitle =
        "No recommendation will be made.\n" +
        "The decision is yours, Captain.";

    [TextArea(2, 4)]
    [SerializeField]
    private string noEnergyCoreSubtitle =
        "Escape launch unavailable.\n" +
        "Energy Core not detected.";

    [TextArea(2, 4)]
    [SerializeField]
    private string returnConfirmedSubtitle =
        "Return protocol confirmed.\n" +
        "Escape pod access granted.";

    [TextArea(2, 4)]
    [SerializeField]
    private string stayConfirmedSubtitle =
        "Command acknowledged.\n" +
        "The Energy Core will remain aboard.";

    [Header("Timing")]
    [SerializeField, Min(0f)]
    private float startDelay = 0.6f;

    [SerializeField, Min(0f)]
    private float gapBetweenVoices = 0.7f;

    [SerializeField, Min(0f)]
    private float imagePreviewDelay = 0.5f;

    [SerializeField, Min(0f)]
    private float subtitleFadeDuration = 0.2f;

    [SerializeField, Min(0f)]
    private float subtitleExtraTime = 0.2f;

    [SerializeField, Min(0.5f)]
    private float fallbackVoiceDuration = 4f;

    [Header("Ending Events")]
    [SerializeField] private UnityEvent returnToEarthEndingEvent;
    [SerializeField] private UnityEvent stayWithShipEndingEvent;

    private bool sequenceStarted;
    private bool sequenceRunning;
    private bool decisionReady;
    private bool decisionMade;
    private bool feedbackRunning;

    private void Awake()
    {
        HideChoices();

        if (decisionImage != null)
        {
            decisionImage.enabled = false;
        }

        if (miloMaintenancePod != null)
        {
            miloMaintenancePod.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (sequenceStarted)
        {
            return;
        }

        StartCoroutine(
            StartWhenPreviousDialogueFinishes()
        );
    }

    private IEnumerator StartWhenPreviousDialogueFinishes()
    {
        while (systemVoiceSource != null &&
               systemVoiceSource.isPlaying)
        {
            yield return null;
        }

        yield return new WaitForSeconds(
            startDelay
        );

        BeginFinalDecision();
    }

    public void BeginFinalDecision()
    {
        if (sequenceStarted)
        {
            return;
        }

        sequenceStarted = true;

        StartCoroutine(
            RunFinalDecisionSequence()
        );
    }

    private IEnumerator RunFinalDecisionSequence()
    {
        sequenceRunning = true;
        decisionReady = false;

        HideChoices();

        bool miloRepaired = IsMiloRepaired();

        if (miloMaintenancePod != null)
        {
            miloMaintenancePod.SetActive(
                miloRepaired
            );
        }

        // Start with the overall summary image.
        ShowImage(
            finalSummaryImage
        );

        // MILO speaks only if it was repaired.
        if (miloRepaired)
        {
            yield return StartCoroutine(
                PlayDialogue(
                    miloVoiceSource,
                    miloReadyVoice,
                    miloReadySubtitle,
                    miloSubtitleColour
                )
            );

            yield return new WaitForSeconds(
                gapBetweenVoices
            );
        }

        // Current ship status.
        yield return StartCoroutine(
            PlayDialogue(
                systemVoiceSource,
                systemIntroVoice,
                systemIntroSubtitle,
                systemSubtitleColour
            )
        );

        yield return new WaitForSeconds(
            gapBetweenVoices
        );

        // Option 01.
        ShowImage(
            returnToEarthImage
        );

        yield return new WaitForSeconds(
            imagePreviewDelay
        );

        yield return StartCoroutine(
            PlayDialogue(
                systemVoiceSource,
                returnToEarthVoice,
                returnToEarthSubtitle,
                systemSubtitleColour
            )
        );

        yield return new WaitForSeconds(
            gapBetweenVoices
        );

        // Option 02.
        ShowImage(
            stayWithShipImage
        );

        yield return new WaitForSeconds(
            imagePreviewDelay
        );

        yield return StartCoroutine(
            PlayDialogue(
                systemVoiceSource,
                stayWithShipVoice,
                stayWithShipSubtitle,
                systemSubtitleColour
            )
        );

        // MILO adds an extra survival possibility.
        if (miloRepaired)
        {
            yield return new WaitForSeconds(
                gapBetweenVoices
            );

            ShowImage(
                finalSummaryImage
            );

            yield return StartCoroutine(
                PlayDialogue(
                    systemVoiceSource,
                    miloRescueSystemVoice,
                    miloRescueSubtitle,
                    systemSubtitleColour
                )
            );
        }

        yield return new WaitForSeconds(
            gapBetweenVoices
        );

        // Return to the neutral summary screen.
        ShowImage(
            finalSummaryImage
        );

        yield return new WaitForSeconds(
            imagePreviewDelay
        );

        yield return StartCoroutine(
            PlayDialogue(
                systemVoiceSource,
                finalDecisionVoice,
                finalDecisionSubtitle,
                systemSubtitleColour
            )
        );

        decisionReady = true;
        sequenceRunning = false;

        ShowChoices();

        Debug.Log(
            "A2: Final decision is now available.",
            this
        );
    }

    public void ChooseReturnToEarth()
    {
        if (!decisionReady ||
            decisionMade ||
            feedbackRunning)
        {
            return;
        }

        if (!HasEnergyCore())
        {
            StartCoroutine(
                ReturnUnavailableSequence()
            );

            return;
        }

        decisionMade = true;
        decisionReady = false;

        HideChoices();

        ShowImage(
            returnToEarthImage
        );

        StartCoroutine(
            ConfirmReturnToEarth()
        );
    }

    public void ChooseStayWithShip()
    {
        if (!decisionReady ||
            decisionMade ||
            feedbackRunning)
        {
            return;
        }

        decisionMade = true;
        decisionReady = false;

        HideChoices();

        ShowImage(
            stayWithShipImage
        );

        StartCoroutine(
            ConfirmStayWithShip()
        );
    }

    private IEnumerator ReturnUnavailableSequence()
    {
        feedbackRunning = true;

        ShowImage(
            returnToEarthImage
        );

        yield return StartCoroutine(
            PlayDialogue(
                systemVoiceSource,
                noEnergyCoreVoice,
                noEnergyCoreSubtitle,
                systemSubtitleColour
            )
        );

        yield return new WaitForSeconds(
            gapBetweenVoices
        );

        ShowImage(
            finalSummaryImage
        );

        feedbackRunning = false;

        Debug.Log(
            "A2: Return to Earth unavailable because the Energy Core was not collected.",
            this
        );
    }

    private IEnumerator ConfirmReturnToEarth()
    {
        yield return StartCoroutine(
            PlayDialogue(
                systemVoiceSource,
                returnConfirmedVoice,
                returnConfirmedSubtitle,
                systemSubtitleColour
            )
        );

        Debug.Log(
            "A2 FINAL DECISION: RETURN TO EARTH.",
            this
        );

        if (returnToEarthEndingEvent != null)
        {
            returnToEarthEndingEvent.Invoke();
        }
    }

    private IEnumerator ConfirmStayWithShip()
    {
        yield return StartCoroutine(
            PlayDialogue(
                systemVoiceSource,
                stayConfirmedVoice,
                stayConfirmedSubtitle,
                systemSubtitleColour
            )
        );

        Debug.Log(
            "A2 FINAL DECISION: STAY WITH THE SHIP.",
            this
        );

        if (stayWithShipEndingEvent != null)
        {
            stayWithShipEndingEvent.Invoke();
        }
    }

    private bool HasEnergyCore()
    {
        return
            A2NarrativeStateManager.Instance != null &&
            A2NarrativeStateManager.Instance.HasEnergyCore;
    }

    private bool IsMiloRepaired()
    {
        return
            A2NarrativeStateManager.Instance != null &&
            A2NarrativeStateManager.Instance.MiloRepaired;
    }

    private void ShowChoices()
    {
        if (returnToEarthButton != null)
        {
            returnToEarthButton.SetActive(true);
        }

        if (stayWithShipButton != null)
        {
            stayWithShipButton.SetActive(true);
        }
    }

    private void HideChoices()
    {
        if (returnToEarthButton != null)
        {
            returnToEarthButton.SetActive(false);
        }

        if (stayWithShipButton != null)
        {
            stayWithShipButton.SetActive(false);
        }
    }

    private void ShowImage(Sprite sprite)
    {
        if (decisionImage == null)
        {
            return;
        }

        decisionImage.sprite = sprite;
        decisionImage.enabled =
            sprite != null;
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
            IsPlaying(miloVoiceSource);
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