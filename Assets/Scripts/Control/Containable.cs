/// <summary>
/// 현재는 B-2 미니게임 냄비용
/// Trigger2D 부착필수
/// 콜리전인 Cutable 객체는 부모만 Cutable, 자식들 잘린것들은 Grabable 필수 부착 = 게임태그가 Cutable임
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Containable : MonoBehaviour
{
    [Header("해당 스테이지에서 담아야하는 갯수")]
    public int _cutAbleNum = 3;
    [Header("디버깅용: 현재 담긴 개수")]
    public int _currentNum = 0;

    public bool _clear = false;
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Cutable"))
        {
            Grabable grabable = collision.gameObject.GetComponent<Grabable>();
            if (grabable._contact == Grabable.ContactState.None) //그랩상태 enum으로 하면 좋겠지만 이게 더 반응빠름
            {
                _currentNum++;
                collision.gameObject.SetActive(false);

            }
        }
    }
    private void Update()
    {
        if(_currentNum >= _cutAbleNum)
        {
            Clear();
        }
    }
    private void Clear()
    {
        Debug.Log("클리어");
        _clear=true;
    }
}
