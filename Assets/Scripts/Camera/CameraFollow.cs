using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    private Transform target;
    private Camera cam;

    private Vector2 minBound;
    private Vector2 maxBound;
    private bool hasBounds;
    private BoxCollider2D boundsCollider;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        var reg = PlayerRegistry.Instance;
        if (reg != null)
        {
            reg.OnPlayerChanged += OnPlayerChanged;
            OnPlayerChanged(reg.CurrentPlayer);
        }
    }

    private void OnDestroy()
    {
        var reg = PlayerRegistry.Instance;
        if (reg != null)
            reg.OnPlayerChanged -= OnPlayerChanged;
    }

    private void OnPlayerChanged(GameObject player)
    {
        target = player ? player.transform : null;

        if (target != null)
            SnapToTarget();
    }

    private void LateUpdate()
    {
        if (!target) return;

        Vector3 desired = new Vector3(
            target.position.x,
            target.position.y,
            transform.position.z
        );

        desired = ClampToBounds(desired);
        transform.position = desired;
    }

    private void SnapToTarget()
    {
        Vector3 snap = new Vector3(
            target.position.x,
            target.position.y,
            transform.position.z
        );

        transform.position = ClampToBounds(snap);
    }

    /// <summary>
    /// 맵 씬의 CameraBounds가 자신의 BoxCollider2D를 등록할 때 호출된다.
    /// </summary>
    public void SetBounds(BoxCollider2D col)
    {
        boundsCollider = col;
        RecalculateBounds();
    }

    /// <summary>
    /// 맵 씬 언로드 시 CameraBounds가 해제될 때 호출된다.
    /// </summary>
    public void ClearBounds()
    {
        boundsCollider = null;
        hasBounds = false;
    }

    private void RecalculateBounds()
    {
        if (!boundsCollider)
        {
            hasBounds = false;
            return;
        }

        Bounds b = boundsCollider.bounds;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        minBound = new Vector2(
            b.min.x + halfWidth,
            b.min.y + halfHeight
        );

        maxBound = new Vector2(
            b.max.x - halfWidth,
            b.max.y - halfHeight
        );

        hasBounds = true;
    }

    private Vector3 ClampToBounds(Vector3 pos)
    {
        if (!hasBounds) return pos;

        pos.x = Mathf.Clamp(pos.x, minBound.x, maxBound.x);
        pos.y = Mathf.Clamp(pos.y, minBound.y, maxBound.y);
        return pos;
    }
}