using UnityEngine;

public class A2NarrativeStateManager : MonoBehaviour
{
    public static A2NarrativeStateManager Instance { get; private set; }

    [Header("Energy Core Choice")]
    [SerializeField] private bool coreDecisionMade;
    [SerializeField] private bool hasEnergyCore;
    [SerializeField] private bool observerRoute;

    [Header("MILO Choice")]
    [SerializeField] private bool miloDecisionMade;
    [SerializeField] private bool miloRepaired;

    public bool CoreDecisionMade => coreDecisionMade;
    public bool HasEnergyCore => hasEnergyCore;
    public bool ObserverRoute => observerRoute;

    public bool MiloDecisionMade => miloDecisionMade;
    public bool MiloRepaired => miloRepaired;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError(
                "More than one A2NarrativeStateManager exists."
            );

            enabled = false;
            return;
        }

        Instance = this;
    }

    public void AcceptEnergyCore()
    {
        if (coreDecisionMade)
        {
            return;
        }

        coreDecisionMade = true;
        hasEnergyCore = true;
        observerRoute = false;

        Debug.Log(
            "A2: Energy Core accepted. Manual control enabled."
        );
    }

    public void ChooseObserverRoute()
    {
        if (coreDecisionMade)
        {
            return;
        }

        coreDecisionMade = true;
        hasEnergyCore = false;
        observerRoute = true;

        Debug.Log(
            "A2: Energy Core refused. Observer Route enabled."
        );
    }

    public void SetMiloDecision(bool repaired)
    {
        if (miloDecisionMade)
        {
            return;
        }

        miloDecisionMade = true;
        miloRepaired = repaired;

        Debug.Log(
            "A2: MILO decision made. Repaired = " +
            miloRepaired
        );
    }
}