using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneName
{
    Title,
    MinigameScene,
    A_1,
    A_2,
    A_3,
    B_1,
    B_2,
    B_3,
    YHouse,
    YHouseRoom, 
    Home,
    BedRoom,
    Whole_Map,
    CHouse,
    Backyard,
    Ending,
    Tutorial,
    
    
}

public class SceneLoader : Singleton<SceneLoader>
{
    [Header("Bootstrap")]
    [SerializeField] private string bootstrapSceneName = "Bootstrap";

    private bool isLoading;

    /// <summary>
    /// 맵 씬을 전환한다. (Bootstrap 씬은 유지)
    /// </summary>
    public void LoadScene(SceneName target, Action onComplete = null)
    {
        if (isLoading) return;

        // Bootstrap 보장 및 씬 전환은 코루틴에서 처리 (target이 이미 로드된 경우도 포함)
        StartCoroutine(LoadSceneAsync(target, onComplete));
    }

    private IEnumerator EnsureBootstrapLoaded()
    {
        if (string.IsNullOrEmpty(bootstrapSceneName)) yield break;
        if (IsSceneLoaded(bootstrapSceneName)) yield break;

        AsyncOperation op = SceneManager.LoadSceneAsync(bootstrapSceneName, LoadSceneMode.Additive);
        while (op != null && !op.isDone) yield return null;
    }

    private IEnumerator LoadSceneAsync(SceneName target, Action onComplete)
    {
        isLoading = true;

        // 0) Bootstrap 씬이 항상 먼저 로드되도록 보장
        yield return EnsureBootstrapLoaded();

        // 현재 활성 씬(대개 “현재 맵 씬”) 저장
        Scene currentActive = SceneManager.GetActiveScene();

        // 1) 타겟 씬이 이미 로드되어 있으면 재로드하지 않고 Active만 전환
        if (!IsSceneLoaded(target.ToString()))
        {
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(target.ToString(), LoadSceneMode.Additive);
            while (loadOp != null && !loadOp.isDone) yield return null;
        }

        // 2) 타겟 씬을 Active로 설정
        Scene targetScene = SceneManager.GetSceneByName(target.ToString());
        SceneManager.SetActiveScene(targetScene);

        // 3) 이전 맵 씬 언로드 (Bootstrap은 절대 언로드 X)
        //    - currentActive가 Bootstrap이면 언로드하지 않음
        if (currentActive.IsValid() && currentActive.name != bootstrapSceneName)
        {
            // 혹시 Title 같은 것도 “맵”으로 취급하면 언로드됨
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentActive);
            while (unloadOp != null && !unloadOp.isDone) yield return null;
        }

        isLoading = false;
        onComplete?.Invoke();
    }

    private bool IsSceneLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == sceneName)
                return true;
        }
        return false;
    }
}