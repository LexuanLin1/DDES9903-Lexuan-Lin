using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public sealed class ElevatorStoryController : MonoBehaviour
{
    [Header("Original Elevator Movement")]
    [SerializeField] private UnityEvent elevatorUpEvent;

    [Header("Voice Sources")]
    [SerializeField] private AudioSource systemVoiceSource;
    [SerializeField] private AudioSource miloVoiceSource;
    [SerializeField] private AudioSource captainVoiceSource;

    [Header("First Ascent")]
    [SerializeField] private AudioClip firstSystemVoice;

    [Header("Second Ascent")]
    [SerializeField] private AudioClip secondSystemVoice;
    [SerializeField] private AudioClip secondMiloVoice;

    [Header("Third Ascent")]
    [SerializeField] private AudioClip thirdSystemVoice;
    [SerializeField] private AudioClip thirdMiloVoice;
    [SerializeField] private AudioClip thirdCaptainVoice;

    [Header("MILO Bridge Move")]
    [SerializeField] private GameObject miloActive;
    [SerializeField] private Transform miloBridgePoint;

    [Header("Timing")]
    [SerializeField, Min(0f)]
    private float storyStartDelay = 0.4f;

    [SerializeField, Min(0f)]
    private float dialogueGap = 0.4f;

    [SerializeField, Min(0f)]
    private float miloMoveDelay = 0.3f;

    private int upPressCount;
    private bool sequenceRunning;

    public void PressUp()
    {
        if (sequenceRunning)
        {
            Debug.Log(
                "A2: Elevator UP temporarily locked. " +
                "Current story sequence is still playing.",
                this
            );

            return;
        }

        if (upPressCount >= 3)
        {
            return;
        }

        upPressCount++;
        sequenceRunning = true;

        Debug.Log(
            "A2: Elevator UP accepted. Press " +
            upPressCount +
            "/3.",
            this
        );

        if (elevatorUpEvent != null)
        {
            elevatorUpEvent.Invoke();
        }

        StartCoroutine(
            PlayCurrentAscentSequence()
        );
    }

    private IEnumerator PlayCurrentAscentSequence()
    {
        yield return new WaitForSeconds(
            storyStartDelay
        );

        if (upPressCount == 1)
        {
            yield return StartCoroutine(
                PlayFirstAscent()
            );
        }
        else if (upPressCount == 2)
        {
            yield return StartCoroutine(
                PlaySecondAscent()
            );
        }
        else if (upPressCount == 3)
        {
            yield return StartCoroutine(
                PlayThirdAscent()
            );
        }

        sequenceRunning = false;

        Debug.Log(
            "A2: Elevator story sequence completed.",
            this
        );
    }

    private IEnumerator PlayFirstAscent()
    {
        yield return StartCoroutine(
            PlayVoiceAndWait(
                systemVoiceSource,
                firstSystemVoice,
                2f
            )
        );
    }

    private IEnumerator PlaySecondAscent()
    {
        // Ship system reports continuing energy loss.
        yield return StartCoroutine(
            PlayVoiceAndWait(
                systemVoiceSource,
                secondSystemVoice,
                2.5f
            )
        );

        // MILO only speaks if the player repaired it.
        if (!IsMiloRepaired())
        {
            yield break;
        }

        yield return new WaitForSeconds(
            dialogueGap
        );

        yield return StartCoroutine(
            PlayVoiceAndWait(
                miloVoiceSource,
                secondMiloVoice,
                2.5f
            )
        );
    }

    private IEnumerator PlayThirdAscent()
    {
        // Final system warning as the lift reaches the upper deck.
        yield return StartCoroutine(
            PlayVoiceAndWait(
                systemVoiceSource,
                thirdSystemVoice,
                2.5f
            )
        );

        // If MILO was not repaired, the sequence simply ends here.
        if (!IsMiloRepaired())
        {
            yield break;
        }

        yield return new WaitForSeconds(
            dialogueGap
        );

        // MILO tells the Captain it will go to the bridge.
        yield return StartCoroutine(
            PlayVoiceAndWait(
                miloVoiceSource,
                thirdMiloVoice,
                2.5f
            )
        );

        yield return new WaitForSeconds(
            dialogueGap
        );

        // Captain acknowledges MILO.
        yield return StartCoroutine(
            PlayVoiceAndWait(
                captainVoiceSource,
                thirdCaptainVoice,
                1.5f
            )
        );

        yield return new WaitForSeconds(
            miloMoveDelay
        );

        MoveMiloToBridge();
    }

    private bool IsMiloRepaired()
    {
        return
            A2NarrativeStateManager.Instance != null &&
            A2NarrativeStateManager.Instance.MiloRepaired;
    }

    private void MoveMiloToBridge()
    {
        if (miloActive == null ||
            miloBridgePoint == null)
        {
            Debug.LogWarning(
                "MILO bridge references are missing.",
                this
            );

            return;
        }

        // MILO was attached to the moving elevator.
        // Detach it before placing it at the bridge.
        miloActive.transform.SetParent(
            null,
            true
        );

        miloActive.transform.SetPositionAndRotation(
            miloBridgePoint.position,
            miloBridgePoint.rotation
        );

        Debug.Log(
            "A2: MILO-01 moved to the Bridge.",
            this
        );
    }

    private IEnumerator PlayVoiceAndWait(
        AudioSource source,
        AudioClip clip,
        float fallbackDuration)
    {
        if (source == null ||
            clip == null)
        {
            yield return new WaitForSeconds(
                fallbackDuration
            );

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
}