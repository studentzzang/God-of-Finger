using UnityEngine;

public class MinigameFlow : Singleton<MinigameFlow>
{
    [SerializeField] private bool hasContext;

    [SerializeField] SceneName returnScene;
    [SerializeField] private string returnSpawnId = "Default";
    [SerializeField] private string successSignalId;
    

    [Header("Runtime")]
    [SerializeField] private bool isExiting;

    public bool IsExiting => isExiting;

    // Clear context only after the return transition really completes.
    private bool waitingReturnTransition;
    private SceneName pendingReturnScene;
    private string pendingReturnSpawnId;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        if (TransitionManager.Instance != null)
            TransitionManager.Instance.OnTransitionCompleted += OnTransitionCompleted;
    }

    public bool HasContext => hasContext;

    public void Begin(SceneName returnScene, string returnSpawnId, string successSignalId = null)
    {   
        this.returnScene = returnScene;
        this.returnSpawnId = string.IsNullOrEmpty(returnSpawnId) ? "Default" : returnSpawnId;
        this.successSignalId = successSignalId;

        hasContext = true;
        isExiting = false;

        waitingReturnTransition = false;
        pendingReturnScene = default;
        pendingReturnSpawnId = null;
    }

    public void Exit(bool success)
    {
        // Prevent re-entrance (e.g., ESC spam during transition)
        if (isExiting) return;

        if (!hasContext)
        {
            Debug.LogWarning("[MinigameFlow] Exit called with no context. Ignored.");
            return;
        }

        if (success && !string.IsNullOrEmpty(successSignalId))
        {
            QuestSignals.Raise(successSignalId);
        }

        var spawner = FindFirstObjectByType<PlayerSpawnSystem>();
        if (spawner != null)
            spawner.SetNextSpawn(returnSpawnId);

        var tm = TransitionManager.Instance;
        if (tm == null)
        {
            Debug.LogWarning("[MinigameFlow] TransitionManager.Instance is null (cannot return)");
            return;
        }

        // If a transition is already running, do NOT clear context.
        // Let the player press ESC again after the current transition finishes.
        if (tm.IsTransitioning)
            return;

        // From here, we will attempt to start a transition.
        isExiting = true;

        // Attempt to start the transition. Do NOT clear context yet; clear only after completion.
        if (tm.TryTransitionTo(returnScene, returnSpawnId))
        {
            waitingReturnTransition = true;
            pendingReturnScene = returnScene;
            pendingReturnSpawnId = returnSpawnId;
            // Keep hasContext=true until OnTransitionCompleted confirms success.
        }
        else
        {
            // Transition request was ignored (already transitioning). Keep context so retry works.
            isExiting = false;
        }
    }

    public void Clear()
    {
        hasContext = false;
        returnScene = default;
        returnSpawnId = "Default";
        successSignalId = null;
    }

    private void OnTransitionCompleted(SceneName scene, string spawnId)
    {
        if (!waitingReturnTransition) return;
        if (scene != pendingReturnScene) return;

        // Return transition really finished -> now it's safe to clear.
        waitingReturnTransition = false;
        Clear();

        // Allow future exits for the next minigame.
        isExiting = false;
    }

    protected override void OnDestroy()
    {
        if (TransitionManager.Instance != null)
            TransitionManager.Instance.OnTransitionCompleted -= OnTransitionCompleted;
        base.OnDestroy();
    }
}