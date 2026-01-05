///<summary>
/// 잡히는 대상 게임 오브젝트에 부착
/// 잡히느 대상 오브젝트는 트리거 콜라이더 필수 부착
///</summary>

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HairInteraction : MonoBehaviour
{
    public Transform targetHand; //손 오브젝트
    Vector3 offset;

    public float _holdTimerLimit = 0.4f;
    public float _holdTimer = 0;

    public float _failTimer = 0;
    public float _failTimerLimit = 1f;

    public bool _catchState = true; //잡기 성공 or 실패

    private int _collide = 0; // 두 손가락 모두 충돌 -> 2되면 잡히기
    private Vector2 _initialPos;
    private void Start()
    {
        SaveInitialPos();
    }
    //개발 상황에 따라 Trigger or Collider 미정
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Finger"))
        {
            _collide++;
        }
        if (_collide >= 2)
        {
            offset = GetOffset(); //최초 1회 offset구하기
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Finger"))
        {
            _collide--;
        }
    }

    void LateUpdate()
    {
        CheckCatch();
        Debug.Log((_holdTimer, _catchState, _collide));
    }
    void SaveInitialPos() //처음 좌표 기억하기
    {
        _initialPos = transform.position;
    }

    //따라다닐 오브젝트와 거리 갭 구하기
    Vector2 GetOffset()
    {
        return transform.position - targetHand.position;
    }
    //0일때 timer 0
    // 1일때 timer +
    //2 일때 
    void CheckCatch()
    {
        if (_collide == 0)
        {
            _catchState = true;
            _holdTimer = 0;
        }
        if (_catchState &&  _collide == 1)
        {
            _holdTimer += Time.deltaTime;

        }
        if (_collide == 2)
        {
            if(_catchState && _holdTimer < _holdTimerLimit)
            {
                _holdTimer = 0;
                _catchState = false;
                Catched();
            }
            else
            {
                FailCatched();
            }   
        }
    }
    void FailCatched()
    {
        _failTimer += Time.deltaTime;
        if(_failTimer > _failTimerLimit)
        {
            _catchState = true;
            return;
        }
    }
    void Catched()
    {
        transform.position = targetHand.position + offset;
       
    }
}
