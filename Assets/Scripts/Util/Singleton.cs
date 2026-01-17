using UnityEngine;

/// <summary>
/// Bootstrap(Additive) 구조에 맞춘 싱글톤:
/// - 자동 생성 X (세팅 누락을 바로 발견)
/// - 중복 방지
/// - DontDestroyOnLoad는 선택 (Bootstrap 씬을 유지하면 보통 필요 없음)
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static bool isQuitting;

    public static T Instance
    {
        get
        {
            if (isQuitting) return null;

            if (instance == null)
            {
                instance = FindFirstObjectByType<T>();
                if (instance == null)
                {
                    Debug.LogError($"[{typeof(T).Name}] Instance를 찾을 수 없습니다. Bootstrap 씬에 배치했는지 확인하세요.");
                }
            }

            return instance;
        }
    }

    [SerializeField] private bool makePersistent = false; // 필요하면 true

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            if (makePersistent)
                DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        isQuitting = true;
    }

    // 씬 전환에서도 OnDestroy는 호출될 수 있으니 isQuitting 세팅 금지
    protected virtual void OnDestroy() { }
}