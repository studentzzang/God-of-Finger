/// <summary>
/// 뽑아야하는 물체에 부착
/// Grabable이랑 같이 부착XX X!@!!
/// Finchable만 붙이기
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finchable : Grabable
{
    public float _gapConditionY = 3.0f;

    private Vector3 _lockedHandPos;
    private Vector3 _lockedObjPos;

    private bool _lockHand = false;
    private bool _lockObject = true;     // 물체 고정 여부
    private bool _isFinchAble = false;

    public bool isClear = false;


    private void Awake()
    {
        _lockedObjPos = transform.position;
    }
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
        base.LateUpdate();

        if (_lockHand)
            targetHand.position = _lockedHandPos;

        if (_lockObject)
            transform.position = _lockedObjPos;

        if (_lockHand) // 잡고 있을 때만 Finch 판정
            CheckFinchAble();

        if (_isFinchAble)
            Finch();
    }
    Vector2 GetMouseWorldPos2D()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = -Camera.main.transform.position.z;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        return new Vector2(worldPos.x, worldPos.y);
    }

    /// <summary>
    ///  잡기상태 유지 -> update에서 lockHand체크
    ///  손가락힘>=마찰력
    ///  마우스포인터가 높게 향해
    /// </summary>
    private void CheckFinchAble()
    {
        Vector2 mousePosToWorld = GetMouseWorldPos2D();

        bool isFingerPowerValid = _fingerPower >= _friction;
        bool isMouseHighEnough =
            (mousePosToWorld.y - targetHand.position.y) >= _gapConditionY;

        if (isFingerPowerValid && isMouseHighEnough)
        {
            _isFinchAble = true;
        }
        else if (!isFingerPowerValid && isMouseHighEnough)
        {
            _isFinchAble = false;

            // Finch 실패 손은 다시 움직이게
            _lockHand = false;

            // 하지만 물체는 계속 고정
            _lockObject = true;
        }
    }


    /// <summary>
    /// 나중에 n초후 뽑힌다는 조건같은게 생기면 여기서구혀ㅑㄴ
    /// </summary>
    private void Finch()
    {
        isClear = true;
        _lockHand = false;
        _lockObject = false;
    }
}
