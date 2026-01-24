using UnityEngine;

public class MinigameFlow : Singleton<MinigameFlow>
{
    [SerializeField] private bool hasContext;

    [SerializeField] SceneName returnScene;
    [SerializeField] private string returnSpawnId = "Default";
    [SerializeField] private string successSignalId;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    public bool HasContext => hasContext;

    public void Begin(SceneName returnScene, string returnSpawnId, string successSignalId = null)
    {
        this.returnScene = returnScene;
        this.returnSpawnId = string.IsNullOrEmpty(returnSpawnId) ? "Default" : returnSpawnId;
        this.successSignalId = successSignalId;

        hasContext = true;
    }

    public void Exit(bool success)
    {
        if (!hasContext)
        {
            Debug.LogWarning("[MinigameFlow] Exit called but no context exists.");
            return;
        }

        if (success && !string.IsNullOrEmpty(successSignalId))
        {
            QuestSignals.Raise(successSignalId);
        }

        var spawner = FindFirstObjectByType<PlayerSpawnSystem>();
        if (spawner != null)
            spawner.SetNextSpawn(returnSpawnId);

        TransitionManager.Instance.TransitionTo(returnScene, returnSpawnId);

        Clear();
    }

    public void Clear()
    {
        hasContext = false;
        returnScene = default;
        returnSpawnId = "Default";
        successSignalId = null;
    }
}