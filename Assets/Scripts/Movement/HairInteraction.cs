///<summary>
/// 미니게임-털 오브젝트에 부착
///</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HairInteraction : MonoBehaviour
{
    public Transform targetHand; //손 오브젝트
    Vector3 offset;

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
    void CheckCatch()
    {
        if (_collide >= 2)
        {
            transform.position = targetHand.position + offset;
        }
    }
}
