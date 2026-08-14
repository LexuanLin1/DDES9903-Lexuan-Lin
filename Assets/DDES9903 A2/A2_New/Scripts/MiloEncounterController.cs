using System.Collections;
using UnityEngine;

public sealed class MiloEncounterController : MonoBehaviour
{
    [Header("MILO States")]
    [SerializeField] private GameObject miloBroken;
    [SerializeField] private GameObject miloActive;

    [Header("Player Choices")]
    [SerializeField] private GameObject repairInteraction;
    [SerializeField] private GameObject leaveInteraction;

    [Header("Path")]
    [SerializeField] private GameObject pathBlocker;
    [SerializeField] private Transform miloSidePoint;

    [Header("MILO Destination")]
    [SerializeField] private Transform miloNextPoint;

    [Header("MILO Light")]
    [SerializeField] private Light attentionLight;

    [Header("Main Voice")]
    [SerializeField] private AudioSource voiceSource;

    [SerializeField] private AudioClip miloDetectedVoice;
    [SerializeField] private AudioClip noCoreVoice;
    [SerializeField] private AudioClip repairVoice;
    [SerializeField] private AudioClip leaveVoice;

    [Header("Repair Dialogue")]
    [SerializeField] private AudioClip captainPhotoQuestionVoice;
    [SerializeField] private AudioClip miloMemoryDamagedVoice;
    [SerializeField] private AudioClip captainJunctionVoice;
    [SerializeField] private AudioClip miloDirectiveAcceptedVoice;

    [Header("Timing")]
    [SerializeField, Min(0.1f)]
    private float moveAsideDuration = 1.5f;

    [SerializeField, Min(0f)]
    private float repairDelay = 0.5f;

    [SerializeField, Min(0f)]
    private float dialogueGap = 0.25f;

    [SerializeField, Min(0f)]
    private float relocationDelay = 0.4f;

    private bool encounterStarted;
    private bool decisionFinished;

    private void Awake()
    {
        encounterStarted = false;
        decisionFinished = false;

        if (miloBroken != null)
        {
            miloBroken.SetActive(true);
        }

        if (miloActive != null)
        {
            miloActive.SetActive(false);
        }

        if (repairInteraction != null)
        {
            repairInteraction.SetActive(false);
        }

        if (leaveInteraction != null)
        {
            leaveInteraction.SetActive(false);
        }

        if (pathBlocker != null)
        {
            pathBlocker.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (encounterStarted)
        {
            return;
        }

        CharacterController player =
            other.GetComponentInParent<CharacterController>();

        if (player == null)
        {
            Transform root = other.transform.root;

            player =
                root.GetComponentInChildren<CharacterController>(
                    true
                );
        }

        if (player == null)
        {
            return;
        }

        encounterStarted = true;

        StartCoroutine(BeginEncounter());
    }

    private IEnumerator BeginEncounter()
    {
        if (A2NarrativeStateManager.Instance == null)
        {
            Debug.LogError(
                "A2NarrativeStateManager was not found.",
                this
            );

            yield break;
        }

        if (A2NarrativeStateManager.Instance.HasEnergyCore)
        {
            PlayVoice(miloDetectedVoice);

            yield return new WaitForSeconds(
                GetClipDuration(
                    miloDetectedVoice,
                    2f
                )
            );

            if (repairInteraction != null)
            {
                repairInteraction.SetActive(true);
            }

            if (leaveInteraction != null)
            {
                leaveInteraction.SetActive(true);
            }

            yield break;
        }

        decisionFinished = true;

        PlayVoice(noCoreVoice);

        yield return new WaitForSeconds(
            GetClipDuration(
                noCoreVoice,
                2f
            )
        );

        A2NarrativeStateManager.Instance.SetMiloDecision(
            false
        );

        yield return StartCoroutine(
            MoveMiloAside()
        );

        FinishEncounter();
    }

    public void RepairMilo()
    {
        if (decisionFinished)
        {
            return;
        }

        if (A2NarrativeStateManager.Instance == null)
        {
            return;
        }

        if (!A2NarrativeStateManager.Instance.HasEnergyCore)
        {
            return;
        }

        decisionFinished = true;

        HideChoiceInteractions();

        A2NarrativeStateManager.Instance.SetMiloDecision(
            true
        );

        StartCoroutine(RepairSequence());
    }

    public void LeaveMilo()
    {
        if (decisionFinished)
        {
            return;
        }

        decisionFinished = true;

        HideChoiceInteractions();

        if (A2NarrativeStateManager.Instance != null)
        {
            A2NarrativeStateManager.Instance.SetMiloDecision(
                false
            );
        }

        StartCoroutine(LeaveSequence());
    }

    private IEnumerator RepairSequence()
    {
        PlayVoice(repairVoice);

        float repairDuration =
            GetClipDuration(
                repairVoice,
                1f
            );

        float actualRepairDelay =
            Mathf.Min(
                repairDelay,
                repairDuration
            );

        yield return new WaitForSeconds(
            actualRepairDelay
        );

        if (miloBroken != null)
        {
            miloBroken.SetActive(false);
        }

        if (miloActive != null)
        {
            miloActive.SetActive(true);
        }

        if (attentionLight != null)
        {
            attentionLight.intensity =
                Mathf.Max(
                    attentionLight.intensity,
                    2f
                );
        }

        float remainingRepairTime =
            repairDuration - actualRepairDelay;

        if (remainingRepairTime > 0f)
        {
            yield return new WaitForSeconds(
                remainingRepairTime
            );
        }

        yield return new WaitForSeconds(dialogueGap);

        yield return StartCoroutine(
            PlayVoiceAndWait(
                captainPhotoQuestionVoice,
                3f
            )
        );

        yield return new WaitForSeconds(dialogueGap);

        yield return StartCoroutine(
            PlayVoiceAndWait(
                miloMemoryDamagedVoice,
                3f
            )
        );

        yield return new WaitForSeconds(dialogueGap);

        yield return StartCoroutine(
            PlayVoiceAndWait(
                captainJunctionVoice,
                2.5f
            )
        );

        yield return new WaitForSeconds(dialogueGap);

        yield return StartCoroutine(
            PlayVoiceAndWait(
                miloDirectiveAcceptedVoice,
                1.5f
            )
        );

        yield return new WaitForSeconds(
            relocationDelay
        );

        MoveMiloToNextPoint();

        FinishEncounter();
    }

    private IEnumerator LeaveSequence()
    {
        PlayVoice(leaveVoice);

        float waitDuration =
            Mathf.Min(
                GetClipDuration(
                    leaveVoice,
                    1f
                ),
                1.5f
            );

        yield return new WaitForSeconds(
            waitDuration
        );

        yield return StartCoroutine(
            MoveMiloAside()
        );

        FinishEncounter();
    }

    private void MoveMiloToNextPoint()
    {
        if (miloActive == null ||
            miloNextPoint == null)
        {
            Debug.LogWarning(
                "MILO next point references are missing.",
                this
            );

            return;
        }

        miloActive.transform.SetPositionAndRotation(
            miloNextPoint.position,
            miloNextPoint.rotation
        );

        Debug.Log(
            "MILO-01 moved to the Central Junction.",
            this
        );
    }

    private IEnumerator MoveMiloAside()
    {
        if (miloBroken == null ||
            miloSidePoint == null)
        {
            yield break;
        }

        Transform target =
            miloBroken.transform;

        Vector3 startPosition =
            target.position;

        Quaternion startRotation =
            target.rotation;

        Vector3 targetPosition =
            miloSidePoint.position;

        Quaternion targetRotation =
            miloSidePoint.rotation;

        float elapsedTime = 0f;

        while (elapsedTime < moveAsideDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsedTime /
                    moveAsideDuration
                );

            float smoothProgress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            target.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    smoothProgress
                );

            target.rotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    smoothProgress
                );

            yield return null;
        }

        target.SetPositionAndRotation(
            targetPosition,
            targetRotation
        );

        if (attentionLight != null)
        {
            attentionLight.intensity = 0f;
        }
    }

    private IEnumerator PlayVoiceAndWait(
        AudioClip clip,
        float fallbackDuration)
    {
        PlayVoice(clip);

        yield return new WaitForSeconds(
            GetClipDuration(
                clip,
                fallbackDuration
            )
        );
    }

    private void FinishEncounter()
    {
        HideChoiceInteractions();

        if (pathBlocker != null)
        {
            pathBlocker.SetActive(false);
        }

        Debug.Log(
            "MILO-01 encounter completed.",
            this
        );
    }

    private void HideChoiceInteractions()
    {
        if (repairInteraction != null)
        {
            repairInteraction.SetActive(false);
        }

        if (leaveInteraction != null)
        {
            leaveInteraction.SetActive(false);
        }
    }

    private void PlayVoice(AudioClip clip)
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

    private float GetClipDuration(
        AudioClip clip,
        float fallback)
    {
        if (clip != null &&
            clip.length > 0f)
        {
            return clip.length;
        }

        return fallback;
    }
}