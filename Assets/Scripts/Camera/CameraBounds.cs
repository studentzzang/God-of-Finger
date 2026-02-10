using UnityEngine;

/// <summary>
/// 맵 씬에 존재하며, 자신의 경계를 카메라 시스템에 등록한다.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class CameraBounds : MonoBehaviour
{
    private void OnEnable()
    {
        var cam = Camera.main;
        if (!cam) return;

        var follow = cam.GetComponent<CameraFollow>();
        if (follow != null)
            follow.SetBounds(GetComponent<BoxCollider2D>());
    }

    private void OnDisable()
    {
        var cam = Camera.main;
        if (!cam) return;

        var follow = cam.GetComponent<CameraFollow>();
        if (follow != null)
            follow.ClearBounds();
    }
}