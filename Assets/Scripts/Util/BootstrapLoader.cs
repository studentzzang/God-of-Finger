using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 어떤 씬에서 시작하더라도 Bootstrap 씬(전역 매니저/UI)을 Additive로 자동 로드한다.
/// </summary>
public class BootstrapLoader : MonoBehaviour
{
    [SerializeField] private string bootstrapSceneName = "Bootstrap";

    private void Awake()
    {
        // 이미 Bootstrap이 로드되어 있으면 아무 것도 하지 않음
        if (IsSceneLoaded(bootstrapSceneName))
        {
            Destroy(gameObject);
            return;
        }

        // Bootstrap Additive 로드
        SceneManager.LoadSceneAsync(bootstrapSceneName, LoadSceneMode.Additive);

        // 로더는 1회용
        Destroy(gameObject);
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