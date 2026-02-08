using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환 파이프라인을 담당하는 매니저.
/// 
/// 역할:
/// - 맵 씬 전환 시 입력 잠금
/// - 페이드 아웃 / 인 처리
/// - 기존 맵 씬 언로드
/// - 다음 맵 씬 Additive 로드
/// - 스폰 위치 적용
/// - (필요 시) 카메라 보정
/// 
/// 부트스트랩 씬(카메라, 전역 매니저)은 유지한 채
/// 맵 씬만 교체하는 구조를 전제로 한다.
/// </summary>
public class TransitionManager : MonoBehaviour
{
    /// <summary>
    /// 전역 접근용 싱글턴 인스턴스
    /// </summary>
    public static TransitionManager Instance { get; private set; }

    /// <summary>
    /// True while a transition coroutine is running.
    /// </summary>
    public bool IsTransitioning => busy;

    /// <summary>
    /// Raised right after a transition is accepted (before fade/unload/load begins).
    /// </summary>
    public event Action<SceneName, string> OnTransitionStarted;

    /// <summary>
    /// Raised when a transition fully completes (after fade-in and input unlock).
    /// </summary>
    public event Action<SceneName, string> OnTransitionCompleted;

    /// <summary>
    /// 화면 페이드 인/아웃을 담당하는 컴포넌트
    /// </summary>
    [SerializeField] private ScreenFader fader;

    /// <summary>
    /// 절대 언로드되지 않아야 하는 부트스트랩 씬 이름
    /// (카메라, 매니저들이 들어 있는 씬)
    /// </summary>
    [Tooltip("Name of the persistent bootstrap scene that contains the camera/managers. This scene will NOT be unloaded during transitions.")]
    [SerializeField] private string bootstrapSceneName = "Bootstrap";

    /// <summary>
    /// 현재 로드되어 있는 맵 씬
    /// 전환 시 언로드 대상이 된다.
    /// </summary>
    private Scene currentMapScene;

    /// <summary>
    /// 전환 중복 실행 방지용 플래그
    /// </summary>
    private bool busy;

    /// <summary>
    /// 다음 씬에서 사용할 스폰 포인트 ID
    /// </summary>

    private void Awake()
    {
        // 싱글턴 보장
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 씬 전환 시에도 유지되도록 설정
        DontDestroyOnLoad(gameObject);

        // 초기 시작 씬을 현재 맵 씬으로 기록
        // (보통 게임 시작 시 첫 맵 씬)
        currentMapScene = SceneManager.GetActiveScene();
    }

    /// <summary>
    /// 외부에서 호출하는 씬 전환 진입 함수
    /// </summary>
    /// <param name="scene">이동할 씬(enum 기반)</param>
    /// <param name="spawnId">도착 후 사용할 스폰 포인트 ID</param>
    public void TransitionTo(SceneName scene, string spawnId)
    {
        TryTransitionTo(scene, spawnId);
    }

    /// <summary>
    /// Attempts to start a transition. Returns true if accepted, false if already transitioning.
    /// </summary>
    public bool TryTransitionTo(SceneName scene, string spawnId)
    {
        // 전환 중이면 추가 전환 요청 무시
        if (busy) return false;

        OnTransitionStarted?.Invoke(scene, spawnId);
        StartCoroutine(Run(scene, spawnId));
        return true;
    }

    /// <summary>
    /// 실제 씬 전환 파이프라인을 실행하는 코루틴
    /// </summary>
    private IEnumerator Run(SceneName scene, string spawnId)
    {
        busy = true;
        var targetScene = scene;
        var targetSpawnId = spawnId;

        try
        {
            // 1. 플레이어 입력 잠금
            PlayerInputLock.SetLocked(true);

            // 2. 화면 페이드 아웃
            if (fader)
                yield return fader.FadeOut();

            // 다음 씬에서 사용할 스폰 ID 저장
            if (PlayerRegistry.Instance != null)
                PlayerRegistry.Instance.SetPendingSpawn(targetSpawnId);
            //플레이어 스폰 위치 적용은 씬 로드 후에 수행

            // 3. 기존 맵 씬 언로드
            // 단, 부트스트랩 씬은 절대 언로드하지 않는다.
            if (currentMapScene.IsValid() &&
                currentMapScene.isLoaded &&
                currentMapScene.name != bootstrapSceneName)
            {
                yield return SceneManager.UnloadSceneAsync(currentMapScene);
            }

            // 4. 다음 맵 씬을 Additive 방식으로 로드
            var nextSceneName = targetScene.ToString();
            yield return SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);

            // 5. 새로 로드된 맵 씬을 Active Scene으로 설정
            currentMapScene = SceneManager.GetSceneByName(nextSceneName);
            if (currentMapScene.IsValid())
            {
                SceneManager.SetActiveScene(currentMapScene);
            }
            yield return null;

            // 6. 화면 페이드 인
            if (fader)
                yield return fader.FadeIn();

            OnTransitionCompleted?.Invoke(targetScene, targetSpawnId);
        }
        finally
        {
            // 7. 플레이어 입력 해제
            PlayerInputLock.SetLocked(false);

            busy = false;
        }
    }
    
}