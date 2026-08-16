using System.Collections;
using UnityEngine;

public sealed class MiloJunctionHint : MonoBehaviour
{
    [Header("MILO")]
    [SerializeField] private GameObject miloActive;
    [SerializeField] private AudioSource miloVoiceSource;

    [Header("Dialogue")]
    [SerializeField] private AudioClip miloArchiveHintVoice;
    [SerializeField] private AudioClip captainLiftInstructionVoice;
    [SerializeField] private AudioClip miloAcknowledgementVoice;

    [Header("Elevator")]
    [SerializeField] private Transform miloElevatorPoint;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float dialogueGap = 0.25f;
    [SerializeField, Min(0f)] private float moveDelay = 0.3f;

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

            player =
                root.GetComponentInChildren<CharacterController>(true);
        }

        if (player == null)
        {
            return;
        }

        if (A2NarrativeStateManager.Instance == null)
        {
            return;
        }

        if (!A2NarrativeStateManager.Instance.MiloRepaired)
        {
            return;
        }

        hasTriggered = true;

        StartCoroutine(
            PlayJunctionSequence()
        );
    }

    private IEnumerator PlayJunctionSequence()
    {
        yield return StartCoroutine(
            PlayVoiceAndWait(
                miloArchiveHintVoice,
                4f
            )
        );

        yield return new WaitForSeconds(
            dialogueGap
        );

        yield return StartCoroutine(
            PlayVoiceAndWait(
                captainLiftInstructionVoice,
                2f
            )
        );

        yield return new WaitForSeconds(
            dialogueGap
        );

        yield return StartCoroutine(
            PlayVoiceAndWait(
                miloAcknowledgementVoice,
                1.5f
            )
        );

        yield return new WaitForSeconds(
            moveDelay
        );

        MoveMiloToElevator();
    }

    private void MoveMiloToElevator()
    {
        if (miloActive == null ||
            miloElevatorPoint == null)
        {
            Debug.LogWarning(
                "MILO elevator references are missing.",
                this
            );

            return;
        }

        miloActive.transform.SetPositionAndRotation(
            miloElevatorPoint.position,
            miloElevatorPoint.rotation
        );

        miloActive.transform.SetParent(
            miloElevatorPoint,
            true
        );

        Debug.Log(
            "MILO-01 moved to the elevator and is now attached to it.",
            this
        );
    }

    private IEnumerator PlayVoiceAndWait(
        AudioClip clip,
        float fallbackDuration)
    {
        if (miloVoiceSource == null ||
            clip == null)
        {
            yield return new WaitForSeconds(
                fallbackDuration
            );

            yield break;
        }

        miloVoiceSource.Stop();
        miloVoiceSource.clip = clip;
        miloVoiceSource.loop = false;
        miloVoiceSource.Play();

        yield return new WaitForSeconds(
            clip.length
        );
    }
}