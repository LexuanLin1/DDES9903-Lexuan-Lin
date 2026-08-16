using UnityEngine;

public sealed class MemoryLockerController : MonoBehaviour
{
    [Header("Memory Voice")]
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioClip memoryVoice;

    [Header("Interaction")]
    [SerializeField] private GameObject openInteraction;

    [Header("Optional Visual Feedback")]
    [SerializeField] private GameObject activatedVisual;

    [Header("Settings")]
    [SerializeField] private bool playOnlyOnce = true;

    private bool hasPlayed;

    public void OpenMemory()
    {
        if (playOnlyOnce && hasPlayed)
        {
            return;
        }

        if (voiceSource == null ||
            memoryVoice == null)
        {
            Debug.LogWarning(
                "Memory Locker voice references are missing.",
                this
            );

            return;
        }

        // Prevent another memory from interrupting the current voice.
        if (voiceSource.isPlaying)
        {
            return;
        }

        hasPlayed = true;

        if (activatedVisual != null)
        {
            activatedVisual.SetActive(true);
        }

        voiceSource.Stop();
        voiceSource.clip = memoryVoice;
        voiceSource.loop = false;
        voiceSource.Play();

        if (playOnlyOnce &&
            openInteraction != null)
        {
            openInteraction.SetActive(false);
        }

        Debug.Log(
            "A2: Optional memory locker opened.",
            this
        );
    }
}