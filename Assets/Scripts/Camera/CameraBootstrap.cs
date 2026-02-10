using UnityEngine;

public static class CameraBootstrap
{
    public static void SnapAndRefresh()
    {
        var cam = Camera.main;
        var player = GameObject.FindWithTag("Player");
        if (!cam || !player) return;

        var p = cam.transform.position;
        p.x = player.transform.position.x;
        p.y = player.transform.position.y;
        cam.transform.position = p;

        // (Cinemachine 쓰면 여기서 Confiner/Follow 재바인딩 위치)
    }
}