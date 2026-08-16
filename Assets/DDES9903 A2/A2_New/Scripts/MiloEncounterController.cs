using System.Collections;
using UnityEngine;

public sealed class MiloEncounterController : MonoBehaviour
{
    [Header("MILO Models")]
    [SerializeField] private GameObject miloBroken;
    [SerializeField] private GameObject miloActive;

    [Header("Interactions")]
    [SerializeField] private GameObject repairInteraction;
    [SerializeField] private GameObject leaveInteraction;

    [Header("Path")]
    [SerializeField] private GameObject pathBlocker;
    [SerializeField] private Transform miloNextPoint;

    [Header("Main Voice")]
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioClip detectedVoice;
    [SerializeField] private AudioClip noCoreVoice;
    [SerializeField] private AudioClip repairVoice;
    [SerializeField] private AudioClip leaveVoice;

    [Header("Repair Dialogue")]
    [SerializeField] private AudioClip captainPhotoQuestionVoice;
    [SerializeField] private AudioClip miloMemoryDamagedVoice;
    [SerializeField] private AudioClip captainJunctionVoice;
    [SerializeField] private AudioClip miloDirectiveAcceptedVoice;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float repairDelay = 0.5f;
    [SerializeField, Min(0f)] private float dialogueGap = 0.25f;
    [SerializeField, Min(0f)] private float relocationDelay = 0.4f;

    private bool encounterRunning;
    private bool miloPermanentlyRepaired;

    private void Start()
    {
        HideChoices();

        if (A2NarrativeStateManager.Instance != null)
        {
            miloPermanentlyRepaired =
                A2NarrativeStateManager.Instance.MiloRepaired;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        CharacterController player =
            other.GetComponentInParent<CharacterController>();

        if (player == null)
        {
            Transform root = other.transform.root;

            player =
                root.GetComponentInChildren<CharacterController>(true);
        }

        if (player == null)
        {
            return;
        }

        if (miloPermanentlyRepaired)
        {
            return;
        }

        BeginEncounter();
    }

    private void BeginEncounter()
    {
        if (encounterRunning)
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

        if (A2NarrativeStateManager.Instance.MiloRepaired)
        {
            miloPermanentlyRepaired = true;
            return;
        }

        encounterRunning = true;

        if (A2NarrativeStateManager.Instance.HasEnergyCore)
        {
            StartCoroutine(CoreAvailableSequence());
        }
        else
        {
            StartCoroutine(NoCoreSequence());
        }
    }

    private IEnumerator CoreAvailableSequence()
    {
        yield return StartCoroutine(
            PlayVoiceAndWait(
                detectedVoice,
                1.5f
            )
        );

        ShowChoices();

        encounterRunning = false;
    }

    private IEnumerator NoCoreSequence()
    {
        HideChoices();

        // MILO stays broken and does not move.
        OpenPath();

        yield return StartCoroutine(
            PlayVoiceAndWait(
                noCoreVoice,
                2f
            )
        );

        encounterRunning = false;

        Debug.Log(
            "A2: MILO repair unavailable. " +
            "The player may return later with the Energy Core.",
            this
        );
    }

    public void RepairMilo()
    {
        if (encounterRunning ||
            miloPermanentlyRepaired)
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
            encounterRunning = true;

            StartCoroutine(
                NoCoreSequence()
            );

            return;
        }

        encounterRunning = true;

        HideChoices();

        StartCoroutine(
            RepairSequence()
        );
    }

    public void LeaveMilo()
    {
        if (encounterRunning ||
            miloPermanentlyRepaired)
        {
            return;
        }

        encounterRunning = true;

        HideChoices();

        StartCoroutine(
            LeaveSequence()
        );
    }

    private IEnumerator RepairSequence()
    {
        // Initial repair system voice.
        yield return StartCoroutine(
            PlayVoiceAndWait(
                repairVoice,
                2f
            )
        );

        yield return new WaitForSeconds(
            repairDelay
        );

        // Replace the broken MILO with the active version.
        if (miloBroken != null)
        {
            miloBroken.SetActive(false);
        }

        if (miloActive != null)
        {
            miloActive.SetActive(true);
        }

        // Record the successful repair.
        if (A2NarrativeStateManager.Instance != null)
        {
            A2NarrativeStateManager.Instance
                .SetMiloDecision(true);
        }

        yield return new WaitForSeconds(
            dialogueGap
        );

        // Captain notices the family photo.
        yield return StartCoroutine(
            PlayVoiceAndWait(
                captainPhotoQuestionVoice,
                2f
            )
        );

        yield return new WaitForSeconds(
            dialogueGap
        );

        // MILO explains its damaged memory.
        yield return StartCoroutine(
            PlayVoiceAndWait(
                miloMemoryDamagedVoice,
                2f
            )
        );

        yield return new WaitForSeconds(
            dialogueGap
        );

        // Captain sends MILO ahead.
        yield return StartCoroutine(
            PlayVoiceAndWait(
                captainJunctionVoice,
                2f
            )
        );

        yield return new WaitForSeconds(
            dialogueGap
        );

        // MILO accepts the instruction.
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

        OpenPath();

        miloPermanentlyRepaired = true;
        encounterRunning = false;

        Debug.Log(
            "A2: MILO-01 repaired and moved to the Central Junction.",
            this
        );
    }

    private IEnumerator LeaveSequence()
    {
        yield return StartCoroutine(
            PlayVoiceAndWait(
                leaveVoice,
                1.5f
            )
        );

        // MILO remains broken in the original position.
        if (miloBroken != null)
        {
            miloBroken.SetActive(true);
        }

        if (miloActive != null)
        {
            miloActive.SetActive(false);
        }

        OpenPath();

        encounterRunning = false;

        Debug.Log(
            "A2: Player left MILO unrepaired. " +
            "MILO can still be repaired if the player returns later.",
            this
        );
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

    private void OpenPath()
    {
        if (pathBlocker != null)
        {
            pathBlocker.SetActive(false);
        }
    }

    private void ShowChoices()
    {
        if (repairInteraction != null)
        {
            repairInteraction.SetActive(true);
        }

        if (leaveInteraction != null)
        {
            leaveInteraction.SetActive(true);
        }
    }

    private void HideChoices()
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

    private IEnumerator PlayVoiceAndWait(
        AudioClip clip,
        float fallbackDuration)
    {
        if (voiceSource == null ||
            clip == null)
        {
            yield return new WaitForSeconds(
                fallbackDuration
            );

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
}