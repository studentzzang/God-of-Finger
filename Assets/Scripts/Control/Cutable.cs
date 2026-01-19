/// <summary>
/// Cut가능한 물체에 들어감, 자식으로 Cut된 객체들이 있어야함
/// Trigger 콜라이더 껴야함
/// </summary>
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

public class Cutable : MonoBehaviour
{
    public bool _clear = false;
    [Header("충돌해야 하는 횟수")]
    public int _cutCountCondition = 3;

    public int _currentCutCount = 0;

    [Header("잘리게 하는 물체 태그명")]
    public string _cutterTagName = "Cutter";

    private void OnTriggerEnter2D(Collider2D other)
    {
     
        if (other.CompareTag(_cutterTagName))
        {
            
            _currentCutCount++;
        }
    }

    private void Update()
    {
        CheckCondition();
        
    }
    void CheckCondition()
    {
        if(_currentCutCount >= _cutCountCondition)
        {
            _clear = true;
                
            ActiveChildren();
            Kill();
        }
    }
    void ActiveChildren()
    {
        for(int i=0; i<transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }
    void Kill()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.enabled = false;

        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // 필요하면 이 스크립트만 끄기
        enabled = false;
    }
}
