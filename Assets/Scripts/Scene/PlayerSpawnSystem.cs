using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bootstrap에 1개만 두는 플레이어 스폰/이동 시스템.
/// - 플레이어는 1개만 유지(DontDestroyOnLoad)
/// - 씬 이동 후, 새 씬의 SpawnPoint로 플레이어 위치를 옮긴다.
/// - (중요) "맵 씬 단독 실행"처럼 첫 로드에서 activeSceneChanged가 안 타는 경우를 대비해
///   Start에서 1회 스폰 정렬을 수행한다.
/// </summary>
public class PlayerSpawnSystem : MonoBehaviour
{
    [SerializeField] private Transform player;                 // Bootstrap에 있는 Player(또는 Tag로 찾음)
    [SerializeField] private string pendingSpawnId = "Default"; // 다음 씬에서 찾을 spawnId

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void Start()
    {
        // "씬을 단독 실행"하면 activeSceneChanged가 한 번도 안 불려서
        // Bootstrap 위치 그대로 시작하는 문제가 생길 수 있음 → 1회 정렬
        StartCoroutine(CoAlignSpawnOnce());
    }

    /// <summary>
    /// 문에서 씬 이동하기 직전에 호출:
    /// 다음 씬에서 어느 SpawnPoint로 이동할지 예약한다.
    /// </summary>
    public void SetNextSpawn(string spawnId)
    {
        pendingSpawnId = string.IsNullOrEmpty(spawnId) ? "Default" : spawnId;
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        AlignToSpawnInScene(newScene);
    }

    private IEnumerator CoAlignSpawnOnce()
    {
        // 한 프레임 대기: 씬의 SpawnPoint들이 생성/초기화된 뒤 찾기
        yield return null;

        AlignToSpawnInScene(SceneManager.GetActiveScene());
    }

    // 현재/새 씬에서 pendingSpawnId → Default 순서로 스폰 위치 정렬
    private void AlignToSpawnInScene(Scene scene)
    {
        // player가 인스펙터에서 빠졌으면 Tag로 복구
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        if (player == null) return;

        var points = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

        SpawnPoint target = null;
        SpawnPoint fallbackDefault = null;

        // scene 소속 SpawnPoint만 대상으로 함 (다른 씬에 남아있는 SpawnPoint 혼선 방지)
        foreach (var p in points)
        {
            if (p == null) continue;
            if (p.gameObject.scene != scene) continue;

            if (p.spawnId == pendingSpawnId) target = p;
            if (p.spawnId == "Default") fallbackDefault = p;
        }

        if (target != null)
        {
            player.position = target.transform.position;
            return;
        }

        if (fallbackDefault != null)
        {
            player.position = fallbackDefault.transform.position;
            return;
        }

        Debug.LogWarning($"[PlayerSpawnSystem] SpawnPoint를 찾지 못함 (spawnId={pendingSpawnId}) in {scene.name}");
    }
}