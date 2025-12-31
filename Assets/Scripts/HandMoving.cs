using UnityEngine;

public class HandMoving : MonoBehaviour
{
    public float smoothTime = 0.15f;
    private Vector3 velocity;

    void Update()
    {
        Vector3 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        target.z = transform.position.z;

        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, smoothTime);
    }
}
