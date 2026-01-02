/// <summary>
/// 의도한 불편한 조작감을 위해 손가락이 자아 없는 물체처럼 움직이는 약간의 물리 효과 줌
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandPhysicsEffect : MonoBehaviour
{
    public Transform _bone;
    public float _stiffness = 120f; // 복원력(클수록 단단)
    public float _damping = 18f; // 감쇠(클수록 덜 흔들림)
    public float _noise = 8f; // 지터 강도(엔트로피)
    public float _maxJitter = 12f;  // 지터 최대 각도

    Quaternion _baseRot;
    float _angVel;     
    float _angle;    

    void Awake()
    {
        _baseRot = _bone.localRotation;
    }

    void LateUpdate()
    {
        float target = Mathf.Clamp(Mathf.PerlinNoise(Time.time * 1.7f, 0f) * 2f - 1f, -1f, 1f);
        target *= _maxJitter;

        float accel = -_stiffness * (_angle - target) - _damping * _angVel;

        accel += (Random.value * 2f - 1f) * _noise;

        _angVel += accel * Time.deltaTime;
        _angle += _angVel * Time.deltaTime;

        _bone.localRotation = _baseRot * Quaternion.AngleAxis(_angle, Vector3.forward);
    }
    
}
