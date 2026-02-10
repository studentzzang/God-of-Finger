/// <summary>
/// Roundable: Grabable 상속
/// - 잡힌 순간 손/물체를 멈춰두고(락)
/// - Grabbing 중 fingerPower == 0 상태가 n초 유지되면 OnRounded() 1회 발동
/// - OnRounded 발동 시 손/물체 락 해제 여부를 Inspector에서 커스텀 가능
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roundable : Grabable
{
    [Tooltip("fingerPower가 0인 상태로 유지되어야 하는 Sec")]
    public float _roundHoldSeconds = 0.5f;

    [Header("Lock 옵션 (잡고 있을 때 멈춰있게)")]
    public bool _lockHandOnGrab = true;
    public bool _lockObjectOnGrab = true;

    [Tooltip("OnRounded 발동 순간 손 락을 풀지 여부")]
    public bool _unlockHandOnRounded = true;

    [Tooltip("OnRounded 발동 순간 물체 락을 풀지 여부")]
    public bool _unlockObjectOnRounded = false;

    [Tooltip("돌아가는 모션 얼마나 돌지 (도) & 클리어")]
    public bool _clear = false;
    public float _rotateDegree = 400; 

    private Vector3 _lockedHandPos;
    private Vector3 _lockedObjPos;

    private bool _isGrabbing = false;
    private bool _roundedTriggered = false;
    private float _roundHoldTimer = 0f;

    private Coroutine _rotateCo;

    protected override void OnGrabbed()
    {
        _isGrabbing = true;
        _roundedTriggered = false;
        _roundHoldTimer = 0f;
        _clear = false;


        // 잡힌 순간 위치 저장
        if (targetHand != null)
            _lockedHandPos = targetHand.position;

        _lockedObjPos = transform.position;
    }

    protected override void OnGrabReleased()
    {
        _isGrabbing = false;
        _roundedTriggered = false;
        _roundHoldTimer = 0f;

        if (_rotateCo != null)
        {
            StopCoroutine(_rotateCo);
            _rotateCo = null;
        }
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        if (!_isGrabbing) return;

        //  잡고 있는 동안 멈춰있게
        if (_lockHandOnGrab && targetHand != null)
            targetHand.position = _lockedHandPos;

        if (_lockObjectOnGrab)
            transform.position = _lockedObjPos;

        if (_roundedTriggered) return;

        if (_fingerPower <= 0)
        {
            _roundHoldTimer += Time.deltaTime;

            if (_roundHoldTimer >= _roundHoldSeconds)
            {
                _roundedTriggered = true;

                if (_unlockHandOnRounded) _lockHandOnGrab = false;
                if (_unlockObjectOnRounded) _lockObjectOnGrab = false;

                OnRounded(); 
            }
        }
        else
        {
            _roundHoldTimer = 0f;

            //  잡는 도중에도 현재 위치로 락 기준점을 갱신하고 주석풀기
            // if (targetHand != null) _lockedHandPos = targetHand.position;
            // _lockedObjPos = transform.position;
        }
    }

    /// <summary>
    /// 최종 Round판정 때 최초1회 발동
    /// </summary>
    protected virtual void OnRounded()
    {
        if (_rotateCo == null)
            _rotateCo = StartCoroutine(RotateFullClear());
    }
    private IEnumerator RotateFullClear()
    {
        float rotated = 0f;

        while (_isGrabbing && rotated < 360f) // 손 놓으면 즉시 종료 + 360도 끝까지
        {
            float step = _rotateDegree * Time.deltaTime;
            step = Mathf.Min(step, 360f - rotated);

            transform.Rotate(0f, 0f, step);
            rotated += step;

            yield return null;
        }

        _rotateCo = null;

        // "끝까지 회전" 성공했을 때만 clear
        if (_isGrabbing && rotated >= 360f)
            _clear = true;
    }

}
