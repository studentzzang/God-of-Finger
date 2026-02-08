using System.Collections;
using UnityEngine;

/// <summary>
/// 맵 씬에 존재하며, 자신의 경계를 카메라 시스템에 등록한다.
/// - 부트스트랩/미니게임 전환 등으로 Camera.main 이 NULL 이 되거나 바뀌는 타이밍이 있어
///   Camera.main 의존을 제거하고 CameraFollow를 직접 찾아 바인딩한다.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class CameraBounds : MonoBehaviour
{
    [Header("Bind Options")]
    [Tooltip("CameraFollow가 늦게 생성/활성화될 수 있어, 일정 시간 동안 기다리며 바인딩합니다.")]
    [SerializeField] private float bindTimeoutSeconds = 2f;

    private BoxCollider2D box;
    private CameraFollow boundFollow;
    private Coroutine bindCo;

    private void Awake()
    {
        box = GetComponent<BoxCollider2D>();
        Debug.Log("[CameraBounds] Awake", this);
    }

    private void OnEnable()
    {
        // 기존 바인딩 코루틴이 있으면 정리
        if (bindCo != null)
            StopCoroutine(bindCo);

        bindCo = StartCoroutine(BindWhenReady());
    }

    private void OnDisable()
    {
        // 내가 실제로 바운드를 걸었던 follow만 해제한다(전환 중 Camera.main이 바뀌는 문제 방지)
        if (boundFollow != null)
            boundFollow.ClearBounds();

        boundFollow = null;

        if (bindCo != null)
        {
            StopCoroutine(bindCo);
            bindCo = null;
        }

        Debug.Log("[CameraBounds] OnDisable", this);
    }

    private IEnumerator BindWhenReady()
    {
        float t = 0f;
        CameraFollow follow = null;

        // Camera.main이 NULL이어도 상관없게 CameraFollow를 직접 찾는다.
        while (t < bindTimeoutSeconds && follow == null)
        {
            follow = FindFirstObjectByType<CameraFollow>();
            if (follow != null)
                break;

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        if (follow == null)
        {
            Debug.LogWarning($"[CameraBounds] Bind failed: CameraFollow not found within {bindTimeoutSeconds:0.##}s", this);
            bindCo = null;
            yield break;
        }

        boundFollow = follow;
        boundFollow.SetBounds(box);
        Debug.Log($"[CameraBounds] Bound to CameraFollow='{boundFollow.name}'", this);

        bindCo = null;
    }
}