using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneName
{
    Title,
    House,
    Street,
    TopdownMovement,
    MinigameScene,
    A_1,
    A_2,
    A_3,
    B_1,
    B_2,
    B_3
    
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

        // 이미 target이 로드되어 있으면 그 씬을 활성화만 하고 끝
        if (IsSceneLoaded(target.ToString()))
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(target.ToString()));
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(LoadSceneAsync(target, onComplete));
    }

    private IEnumerator LoadSceneAsync(SceneName target, Action onComplete)
    {
        isLoading = true;

        // 현재 활성 씬(대개 “현재 맵 씬”) 저장
        Scene currentActive = SceneManager.GetActiveScene();

        // 1) 타겟 씬 Additive 로드
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(target.ToString(), LoadSceneMode.Additive);
        while (!loadOp.isDone) yield return null;

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