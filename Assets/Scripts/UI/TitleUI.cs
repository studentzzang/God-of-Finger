using UnityEngine;

public class TitleUI : MonoBehaviour
{
    [Header("Start Destination")]
    [SerializeField] private SceneName startScene = SceneName.House;
    [SerializeField] private string startSpawnPointId = "Default";

    [Header("New Game Options")]
    [SerializeField] private bool clearRuntimeOnceFlagsOnStart = true;

    public void OnClickStart()
    {
        if (TransitionManager.Instance == null)
        {
            Debug.LogError("[TitleUI] TransitionManager.Instance is null. Is Bootstrap loaded?");
            return;
        }

        if (clearRuntimeOnceFlagsOnStart && RuntimeOnceFlags.Instance != null)
        {
            RuntimeOnceFlags.Instance.ClearAll();
        }

        TransitionManager.Instance.TransitionTo(startScene, startSpawnPointId);
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }
}