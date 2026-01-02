using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandContol : MonoBehaviour
{
    public Transform _firstFinger;
    public Transform _secondFinger;

    float _firstFingerAngle = 0f;
    float _secondFingerAngle = 0f;

    Quaternion _firstFingerBaseLocalRot;
    Quaternion _secondFingerBaseLocalRot;

    public float _boneRotationSpeed = 15f;
    public float _maxCurlDegree = 70; //손가락 접혀지는 최대 각도(도)
    float _curl = 0;

    void Awake()
    {
        if (_firstFinger) _firstFingerBaseLocalRot = _firstFinger.localRotation;
        if (_secondFinger) _secondFingerBaseLocalRot = _secondFinger.localRotation;
    }

    void Update()
    {
        KeyCheck();
    }
    void KeyCheck()
    {
        // second(검지 등) : +1이 "접기"라고 가정
        if (Input.GetKey(KeyCode.V))
            CurlFinger(_secondFinger, ref _secondFingerAngle, 1);
        else if (_secondFingerAngle > 0f)
            CurlFinger(_secondFinger, ref _secondFingerAngle, -1);

        // thumb(엄지) : 방향 반대라서 "접기"는 -1
        if (Input.GetKey(KeyCode.Space))
            CurlFinger(_firstFinger, ref _firstFingerAngle, 1);
        else if (_firstFingerAngle > 0f)
            CurlFinger(_firstFinger, ref _firstFingerAngle, -1);
    }

    /// <summary>
    ///  손가락 모으기 fingerName에 전역변수 bone name
    ///  dir은 -1또는 1 / 왼쪽 또는 오른쪽
    /// </summary>

    void CurlFinger(Transform bone, ref float currentAngle, int dir)
    {
        if (!bone) return;

        float delta = _boneRotationSpeed * Time.deltaTime * dir;
        float nextAngle = currentAngle + delta;

        // 최대/최소 각도 제한
        nextAngle = Mathf.Clamp(nextAngle, 0f, _maxCurlDegree);

        currentAngle = nextAngle;

        float sign = (bone == _firstFinger) ? -1f : 1f;
        Quaternion baseRot = (bone == _firstFinger) ? _firstFingerBaseLocalRot : _secondFingerBaseLocalRot;

        bone.localRotation = baseRot * Quaternion.AngleAxis(currentAngle * sign, Vector3.forward);
    }

    void DetechFinger() //손가락 모으다가 해제
    {

    }
}
