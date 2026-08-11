using System.Collections;
using UnityEngine;

public sealed class MissionSecurityController : MonoBehaviour
{
    [Header("Infinite Corridor")]
    [SerializeField] private GameObject loopTrigger;

    [Header("Alarm")]
    [SerializeField] private AudioSource alarmAudio;
    [SerializeField] private GameObject[] alarmVisuals;
    [SerializeField, Min(0.05f)] private float flashInterval = 0.32f;

    [Header("Energy Vault")]
    [SerializeField] private GameObject[] energyBarriers;
    [SerializeField] private GameObject[] energyBlockers;
    [SerializeField] private GameObject dispenserInteraction;

    public bool SecurityActive { get; private set; }
    public bool AuthenticationComplete { get; private set; }

    private Coroutine flashRoutine;
    private bool firstLoopCompleted;

    private void Awake()
    {
        SecurityActive = false;
        AuthenticationComplete = false;
        firstLoopCompleted = false;

        SetObjectActive(loopTrigger, false);
        SetObjectsActive(alarmVisuals, false);

        SetObjectsActive(energyBarriers, true);
        SetObjectsActive(energyBlockers, true);
        SetObjectActive(dispenserInteraction, false);

        if (alarmAudio != null)
        {
            alarmAudio.Stop();
        }
    }

    public void BeginSecurityProtocol()
    {
        if (SecurityActive || AuthenticationComplete)
        {
            return;
        }

        SecurityActive = true;

        SetObjectActive(loopTrigger, true);

        if (alarmAudio != null &&
            alarmAudio.clip != null &&
            !alarmAudio.isPlaying)
        {
            alarmAudio.Play();
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashAlarm());
    }

    public void RegisterCorridorLoop()
    {
        if (firstLoopCompleted)
        {
            return;
        }

        firstLoopCompleted = true;

        Debug.Log(
            "First corridor loop completed. Return to the observation deck."
        );
    }

    public void CompleteAuthentication()
    {
        if (AuthenticationComplete)
        {
            return;
        }

        AuthenticationComplete = true;
        SecurityActive = false;

        SetObjectActive(loopTrigger, false);

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        SetObjectsActive(alarmVisuals, false);

        if (alarmAudio != null)
        {
            alarmAudio.Stop();
        }

        SetObjectsActive(energyBarriers, false);
        SetObjectsActive(energyBlockers, false);
        SetObjectActive(dispenserInteraction, true);

        Debug.Log(
            "Identity verified. Security protocol disabled."
        );
    }

    private IEnumerator FlashAlarm()
    {
        bool lightsOn = true;

        while (SecurityActive)
        {
            SetObjectsActive(alarmVisuals, lightsOn);
            lightsOn = !lightsOn;

            yield return new WaitForSeconds(flashInterval);
        }

        SetObjectsActive(alarmVisuals, false);
    }

    private static void SetObjectActive(
        GameObject target,
        bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

    private static void SetObjectsActive(
        GameObject[] targets,
        bool active)
    {
        if (targets == null)
        {
            return;
        }

        foreach (GameObject target in targets)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
