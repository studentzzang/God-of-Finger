/// <summary>
/// 뽑아야하는 물체에 부착
/// Grabable이랑 같이 부착XXX!@!!
/// Finchable만 붙이기
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finchable : Grabable
{
    private Vector3 _lockedHandPos;
    private Vector3 _lockedObjPos;

    public float _gapConditionY = 3.0f;
    private bool _lockHand = false;

    // 잡기 성공 순간
    protected override void OnGrabbed()
    {
        _lockedHandPos = targetHand.position;
        _lockedObjPos = transform.position;   
        _lockHand = true;
    }

    protected override void OnGrabReleased()
    {
        _lockHand = false;
    }

    protected override void LateUpdate()
    {
        Debug.Log($"lockHand:{_lockHand}");
        base.LateUpdate();
        if (!_lockHand) return;

        // 손 고정
        targetHand.position = _lockedHandPos;

        transform.position = _lockedObjPos;
    }
}
