using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandControl : MonoBehaviour
{
    public Transform _firstFinger;
    public Transform _secondFinger;

    public MonoBehaviour _firstPhysicsScript;
    public MonoBehaviour _secondPhysicsScript;

    float _firstFingerAngle = 0f;
    float _secondFingerAngle = 0f;

    Quaternion _firstFingerBaseLocalRot;
    Quaternion _secondFingerBaseLocalRot;

    public float _boneRotationSpeed = 15f;
    public float _maxCurlDegree = 70; //손가락 접혀지는 최대 각도(도)
    float _curl = 0;

    public bool _inputV = false;
    public bool _inputSpace = false;

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

        if (Input.GetKey(KeyCode.V)) _inputV = true;
        else _inputV = false;

        if (Input.GetKey(KeyCode.Space)) _inputSpace = true;
        else _inputSpace = false;
        // second(검지)
        if (_inputV)
        {
            CurlFinger(_secondFinger, ref _secondFingerAngle, 1);
            if (_secondPhysicsScript) _secondPhysicsScript.enabled = false;
        }
        else if (_secondFingerAngle > 0f)
        {
            CurlFinger(_secondFinger, ref _secondFingerAngle, -1);
        }
        else
        {
            if (_secondPhysicsScript) _secondPhysicsScript.enabled = true;
        }

        // thumb(엄지) 
        if (_inputSpace)
        {
            CurlFinger(_firstFinger, ref _firstFingerAngle, 1);
            if (_firstPhysicsScript) _firstPhysicsScript.enabled = false;
        }
        else if (_firstFingerAngle > 0f)
        {
            CurlFinger(_firstFinger, ref _firstFingerAngle, -1);
        }
        else
        {
            if (_firstPhysicsScript) _firstPhysicsScript.enabled = true;
        }
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
