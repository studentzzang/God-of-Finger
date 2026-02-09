using UnityEngine;

public static class MouseClamp
{
    public static Vector3 GetClampedMousePosition()
    {
        Vector3 p = Input.mousePosition;

        p.x = Mathf.Clamp(p.x, 0f, Screen.width);
        p.y = Mathf.Clamp(p.y, 0f, Screen.height);

        return p;
    }
}
