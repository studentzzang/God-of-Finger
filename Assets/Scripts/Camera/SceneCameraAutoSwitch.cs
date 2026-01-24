using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneCameraAutoSwitch : MonoBehaviour
{
    [SerializeField] private CameraRouter router;

    private void Awake()
    {
        if (router == null)
            router = GetComponent<CameraRouter>();

        if (router == null)
            router = FindFirstObjectByType<CameraRouter>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // 첫 진입 씬에서도 한번 적용
        ApplyForScene(SceneManager.GetActiveScene());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 오브젝트 초기화 순서(awake/start) 때문에 1프레임 늦춰서 판정하면 안정적이다.
        StartCoroutine(CoApplyNextFrame(scene));
    }

    private IEnumerator CoApplyNextFrame(Scene scene)
    {
        yield return null;
        ApplyForScene(scene);
    }

    private void ApplyForScene(Scene scene)
    {
        if (router == null)
        {
            Debug.LogWarning("[AutoSwitch] router is null");
            return;
        }

        bool isWorld = HasWorldMarkerInScene(scene);
        Debug.Log($"[AutoSwitch] loadedScene={scene.name} isWorld={isWorld} router={router.name}");

        if (isWorld) router.UseWorld();
        else router.UseMinigame();
    }

    private bool HasWorldMarkerInScene(Scene scene)
    {
        var markers = Object.FindObjectsByType<WorldSceneMarker>(FindObjectsSortMode.None);
        foreach (var m in markers)
        {
            if (m != null && m.gameObject.scene == scene)
                return true;
        }
        return false;
    }
}