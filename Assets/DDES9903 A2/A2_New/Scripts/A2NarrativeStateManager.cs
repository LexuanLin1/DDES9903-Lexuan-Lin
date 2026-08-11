using UnityEngine;

public enum A2Room
{
    None,
    CryogenicTransferBay,
    CrewMemoryArchive,
    LongRangeCommunications
}

public enum A2FinalChoice
{
    None,
    Escape,
    Stay
}

public class A2NarrativeStateManager : MonoBehaviour
{
    public static A2NarrativeStateManager Instance { get; private set; }

    [Header("Energy Core")]
    [SerializeField] private bool coreDecisionMade;
    [SerializeField] private bool hasEnergyCore;
    [SerializeField] private bool observerRoute;

    [Header("MILO")]
    [SerializeField] private bool miloDecisionMade;
    [SerializeField] private bool miloRepaired;

    [Header("Chambers")]
    [SerializeField] private int chambersCompleted;
    [SerializeField] private A2Room firstChamber = A2Room.None;
    [SerializeField] private A2Room secondChamber = A2Room.None;

    [Header("Second Floor")]
    [SerializeField] private bool bridgeUnlocked;

    [Header("Final Choice")]
    [SerializeField] private A2FinalChoice finalChoice = A2FinalChoice.None;

    public bool CoreDecisionMade => coreDecisionMade;
    public bool HasEnergyCore => hasEnergyCore;
    public bool ObserverRoute => observerRoute;

    public bool MiloDecisionMade => miloDecisionMade;
    public bool MiloRepaired => miloRepaired;

    public int ChambersCompleted => chambersCompleted;
    public A2Room FirstChamber => firstChamber;
    public A2Room SecondChamber => secondChamber;

    public bool BridgeUnlocked => bridgeUnlocked;
    public A2FinalChoice FinalChoice => finalChoice;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("More than one A2NarrativeStateManager exists in the scene.");
            enabled = false;
            return;
        }

        Instance = this;
    }
}
