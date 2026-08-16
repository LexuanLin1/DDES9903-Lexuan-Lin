using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BlackBoxArchiveController : MonoBehaviour
{
    [Header("Screen")]
    [SerializeField] private TMP_Text screenText;
    [SerializeField] private RawImage screenImage;

    [Header("Archive Images")]
    [SerializeField] private Texture crewImage01;
    [SerializeField] private Texture crewImage02;
    [SerializeField] private Texture crewImage03;

    [Header("Voice")]
    [SerializeField] private AudioSource voiceSource;

    [SerializeField] private AudioClip energyCoreRequiredVoice;

    [SerializeField] private AudioClip crewMessage01;
    [SerializeField] private AudioClip crewMessage02;
    [SerializeField] private AudioClip crewMessage03;

    [SerializeField] private AudioClip captainReflectionVoice;

    [Header("Timing")]
    [SerializeField, Min(0f)]
    private float restoringDuration = 1.5f;

    [SerializeField, Min(0f)]
    private float imageGap = 0.5f;

    [SerializeField, Min(0f)]
    private float reflectionDelay = 4f;

    private bool isRunning;

    private void Start()
    {
        ShowIdleScreen();
    }

    public void StartBlackBoxArchive()
    {
        if (isRunning)
        {
            return;
        }

        if (A2NarrativeStateManager.Instance == null)
        {
            Debug.LogError(
                "A2NarrativeStateManager was not found.",
                this
            );

            return;
        }

        if (!A2NarrativeStateManager.Instance.HasEnergyCore)
        {
            ShowEnergyCoreRequired();
            return;
        }

        StartCoroutine(
            PlayArchiveSequence()
        );
    }

    private IEnumerator PlayArchiveSequence()
    {
        isRunning = true;

        if (screenImage != null)
        {
            screenImage.enabled = false;
        }

        if (screenText != null)
        {
            screenText.text =
                "BLACK BOX ARCHIVE\n\n" +
                "RESTORING DAMAGED DATA...";
        }

        yield return new WaitForSeconds(
            restoringDuration
        );

        // Message 01
        ShowArchiveFrame(
            crewImage01,
            "RECOVERED CREW MESSAGE\n\n" +
            "TIMESTAMP: 5 YEARS AGO"
        );

        yield return StartCoroutine(
            PlayVoiceAndWait(
                crewMessage01
            )
        );

        yield return new WaitForSeconds(
            imageGap
        );

        // Message 02
        ShowArchiveFrame(
            crewImage02,
            "EMERGENCY POWER FAILURE\n\n" +
            "CRYOSLEEP SYSTEM RECORD"
        );

        yield return StartCoroutine(
            PlayVoiceAndWait(
                crewMessage02
            )
        );

        yield return new WaitForSeconds(
            imageGap
        );

        // Message 03
        ShowArchiveFrame(
            crewImage03,
            "FINAL CREW MESSAGE\n\n" +
            "RECORDED 5 YEARS AGO"
        );

        yield return StartCoroutine(
            PlayVoiceAndWait(
                crewMessage03
            )
        );

        // End archive.
        if (screenImage != null)
        {
            screenImage.enabled = false;
        }

        if (screenText != null)
        {
            screenText.text =
                "CREW MESSAGE ENDED\n\n" +
                "ARCHIVE TIMESTAMP:\n" +
                "5 YEARS AGO";
        }

        yield return new WaitForSeconds(
            reflectionDelay
        );

        // Captain reflects on what was discovered.
        yield return StartCoroutine(
            PlayVoiceAndWait(
                captainReflectionVoice
            )
        );

        isRunning = false;

        Debug.Log(
            "A2: Black Box archive sequence completed.",
            this
        );
    }

    private void ShowArchiveFrame(
        Texture image,
        string text)
    {
        if (screenImage != null)
        {
            screenImage.texture = image;
            screenImage.enabled = image != null;
        }

        if (screenText != null)
        {
            screenText.text = text;
        }
    }

    private void ShowEnergyCoreRequired()
    {
        if (screenImage != null)
        {
            screenImage.enabled = false;
        }

        if (screenText != null)
        {
            screenText.text =
                "BLACK BOX OFFLINE\n\n" +
                "ENERGY CORE REQUIRED";
        }

        PlayVoice(
            energyCoreRequiredVoice
        );

        Debug.Log(
            "A2: Black Box unavailable. Energy Core required.",
            this
        );
    }

    private void ShowIdleScreen()
    {
        if (screenImage != null)
        {
            screenImage.enabled = false;
        }

        if (screenText != null)
        {
            screenText.text =
                "BLACK BOX ARCHIVE\n\n" +
                "SYSTEM STANDBY";
        }
    }

    private IEnumerator PlayVoiceAndWait(
        AudioClip clip)
    {
        if (voiceSource == null ||
            clip == null)
        {
            yield break;
        }

        voiceSource.Stop();
        voiceSource.clip = clip;
        voiceSource.loop = false;
        voiceSource.Play();

        yield return new WaitForSeconds(
            clip.length
        );
    }

    private void PlayVoice(
        AudioClip clip)
    {
        if (voiceSource == null ||
            clip == null)
        {
            return;
        }

        voiceSource.Stop();
        voiceSource.clip = clip;
        voiceSource.loop = false;
        voiceSource.Play();
    }
}