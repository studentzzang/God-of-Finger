using UnityEngine;

public class HandMoving : MonoBehaviour
{
    public float smoothTime = 0.15f;
    private Vector3 velocity;

    void Update()
    {
        Vector3 mouse = Input.mousePosition;

        mouse.x = Mathf.Clamp(mouse.x, 0f, Screen.width);
        mouse.y = Mathf.Clamp(mouse.y, 0f, Screen.height);

        Vector3 target = Camera.main.ScreenToWorldPoint(mouse);
        target.z = transform.position.z;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            target,
            ref velocity,
            smoothTime
        );
    }
}
