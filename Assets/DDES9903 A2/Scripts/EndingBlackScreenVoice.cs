using System.Collections;
using UnityEngine;

public sealed class EndingBlackScreenVoice : MonoBehaviour
{
    [Header("Daughter Voice")]
    [SerializeField]
    private AudioSource voiceSource;

    [SerializeField]
    private AudioClip daughterVoice;

    [Header("Timing")]
    [SerializeField, Min(0f)]
    private float voiceDelay = 1.5f;

    private bool hasPlayed;

    private void OnEnable()
    {
        if (hasPlayed)
        {
            return;
        }

        hasPlayed = true;

        StartCoroutine(
            PlayDaughterVoice()
        );
    }

    private IEnumerator PlayDaughterVoice()
    {
        yield return new WaitForSeconds(
            voiceDelay
        );

        if (voiceSource == null ||
            daughterVoice == null)
        {
            yield break;
        }

        voiceSource.Stop();
        voiceSource.clip = daughterVoice;
        voiceSource.loop = false;
        voiceSource.Play();
    }
}