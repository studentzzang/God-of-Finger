/// <summary>
/// 펄린 노이즈 사용해서 ㄷㄷ 떨리는 효과
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalJitter : MonoBehaviour
{
    public FingerPower _fingerPowerScript;
    public int _fingerPower;
    public float strength = 0.03f; // 떨림 강도
    public float speed = 20f;      // 떨림 속도

    Vector3 originPos;

    void Start()
    {
        _fingerPowerScript = GetComponentInParent<FingerPower>();
        originPos = transform.localPosition;
    }

    void Update()
    {
        _fingerPower = _fingerPowerScript._power;
        float x = Mathf.PerlinNoise(Time.time * speed, 0f) - 0.5f;
        float y = Mathf.PerlinNoise(0f, Time.time * speed) - 0.5f;

        transform.localPosition = originPos + new Vector3(x, y, 0) * strength * _fingerPower;
    }
}
