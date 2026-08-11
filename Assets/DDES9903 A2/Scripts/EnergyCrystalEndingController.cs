using System.Collections;
using UnityEngine;

public sealed class EnergyCellEndingController : MonoBehaviour
{
    [Header("Alarm")]
    public AudioSource alarmSource;

    [Header("Alarm Light Groups")]
    public GameObject[] alarmLightGroups;

    [Header("Flashing Timing")]
    public float lightOnDuration = 0.3f;
    public float lightOffDuration = 0.2f;

    [Header("Pickup Dialogue")]
    public AudioSource systemVoiceSource;
    public AudioSource playerVoiceSource;

    public AudioClip energyLowVoice;
    public AudioClip returnToCryoVoice;

    public float alarmLeadIn = 0.5f;
    public float gapBetweenVoices = 0.25f;

    [Range(0f, 1f)]
    public float dialogueAlarmVolume = 0.08f;

    [Header("Ending")]
    public AudioSource endingVoiceSource;
    public AudioClip endingVoice;
    public float endingVoiceDelay = 0.5f;

    [Header("Optional Ending Screen")]
    public GameObject endingScreen;

    public bool EmergencyActive { get; private set; }
    public bool EndingTriggered { get; private set; }

    private float originalAlarmVolume;

    private Coroutine flashingRoutine;
    private Coroutine pickupDialogueRoutine;

    private void Awake()
    {
        EmergencyActive = false;
        EndingTriggered = false;

        if (alarmSource != null)
        {
            originalAlarmVolume = alarmSource.volume;
            alarmSource.Stop();
            alarmSource.loop = true;
        }

        SetAlarmLights(false);

        if (endingScreen != null)
        {
            endingScreen.SetActive(false);
        }
    }

    public void StartEmergency()
    {
        if (EmergencyActive || EndingTriggered)
        {
            return;
        }

        EmergencyActive = true;

        if (alarmSource != null)
        {
            originalAlarmVolume = alarmSource.volume;
            alarmSource.loop = true;

            if (!alarmSource.isPlaying)
            {
                alarmSource.Play();
            }
        }

        flashingRoutine = StartCoroutine(
            FlashAlarmLights()
        );

        pickupDialogueRoutine = StartCoroutine(
            PlayPickupDialogue()
        );
    }

    public void TriggerEnding()
    {
        if (!EmergencyActive || EndingTriggered)
        {
            return;
        }

        StartCoroutine(
            RunEndingSequence()
        );
    }

    private IEnumerator FlashAlarmLights()
    {
        while (EmergencyActive && !EndingTriggered)
        {
            SetAlarmLights(true);

            yield return new WaitForSeconds(
                lightOnDuration
            );

            SetAlarmLights(false);

            yield return new WaitForSeconds(
                lightOffDuration
            );
        }

        SetAlarmLights(false);
    }

    private IEnumerator PlayPickupDialogue()
    {
        yield return new WaitForSeconds(
            alarmLeadIn
        );

        LowerAlarmVolume();

        yield return StartCoroutine(
            PlayVoiceAndWait(
                systemVoiceSource,
                energyLowVoice
            )
        );

        yield return new WaitForSeconds(
            gapBetweenVoices
        );

        yield return StartCoroutine(
            PlayVoiceAndWait(
                playerVoiceSource,
                returnToCryoVoice
            )
        );

        RestoreAlarmVolume();

        pickupDialogueRoutine = null;
    }

    private IEnumerator RunEndingSequence()
    {
        EndingTriggered = true;
        EmergencyActive = false;

        if (pickupDialogueRoutine != null)
        {
            StopCoroutine(pickupDialogueRoutine);
            pickupDialogueRoutine = null;
        }

        StopVoice(systemVoiceSource);
        StopVoice(playerVoiceSource);

        if (flashingRoutine != null)
        {
            StopCoroutine(flashingRoutine);
            flashingRoutine = null;
        }

        RestoreAlarmVolume();

        if (alarmSource != null)
        {
            alarmSource.Stop();
        }

        SetAlarmLights(false);

        yield return new WaitForSeconds(
            endingVoiceDelay
        );

        yield return StartCoroutine(
            PlayVoiceAndWait(
                endingVoiceSource,
                endingVoice
            )
        );

        if (endingScreen != null)
        {
            endingScreen.SetActive(true);
        }
    }

    private IEnumerator PlayVoiceAndWait(
        AudioSource source,
        AudioClip clip)
    {
        if (source == null || clip == null)
        {
            yield break;
        }

        source.Stop();
        source.clip = clip;
        source.loop = false;
        source.Play();

        yield return new WaitForSeconds(
            clip.length
        );
    }

    private void LowerAlarmVolume()
    {
        if (alarmSource == null)
        {
            return;
        }

        alarmSource.volume = Mathf.Min(
            originalAlarmVolume,
            dialogueAlarmVolume
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

    private void SetAlarmLights(bool active)
    {
        if (alarmLightGroups == null)
        {
            return;
        }

        foreach (GameObject lightGroup in alarmLightGroups)
        {
            if (lightGroup != null)
            {
                lightGroup.SetActive(active);
            }
        }
    }

    private static void StopVoice(
        AudioSource source)
    {
        if (source != null &&
            source.isPlaying)
        {
            source.Stop();
        }
    }

    private void OnDisable()
    {
        RestoreAlarmVolume();
        SetAlarmLights(false);
    }
}