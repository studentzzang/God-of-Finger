using UnityEngine;

/// <summary>
/// Bootstrap(Additive) 구조용 싱글톤
/// - 자동 생성 X
/// - Instance getter에서 Find로 "찾아주지 않음" (의도적으로 null 가능)
/// - Awake에서만 등록
/// - 중복 방지
/// - (선택) DontDestroyOnLoad
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static bool isQuitting;

    /// <summary>
    /// 반드시 존재한다고 "보장"되는 상황에서만 사용.
    /// 없으면 null 반환(또는 원하는 경우 예외/로그).
    /// </summary>
    public static T Instance
    {
        get
        {
            if (isQuitting) return null;
            return instance;
        }
    }

    /// <summary>
    /// "없을 수도" 있는 상황에서 안전하게 체크하는 용도.
    /// </summary>
    public static T InstanceOrNull => isQuitting ? null : instance;

    public static bool HasInstance => !isQuitting && instance != null;

    [SerializeField] private bool makePersistent = false;

    protected virtual void Awake()
    {
        if (isQuitting)
        {
            Destroy(gameObject);
            return;
        }

        if (instance == null)
        {
            instance = this as T;

            if (makePersistent)
                DontDestroyOnLoad(gameObject);

            return;
        }

        if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        isQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        // 진짜 종료가 아니라 씬 언로드/중복 제거로 Destroy 되는 경우가 많아서
        // isQuitting은 여기서 건드리면 안 됨.
        if (!isQuitting && instance == this)
        {
            // Bootstrap 씬이 언로드되는 상황(테스트 등)에서는 instance를 비워주는 게 안전
            instance = null;
        }
    }
}