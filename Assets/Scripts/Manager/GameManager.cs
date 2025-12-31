using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameObject talkbox;
    [SerializeField] private TextMeshProUGUI talkText;
    private bool isAction = false;
    public bool IsAction => isAction;
        


    //summary>
    /// 간단한 플레이어 상호작용 구현
    /// <param name="obj">상호작용 대상 오브젝트</param>
    /// </summary>
    public void Action(GameObject obj)
    {
        if (isAction) //대화중이면 탈출
        {
            isAction = false;
            talkbox.SetActive(false);
            //talkText.gameObject.SetActive(false);
            
        }
        else //아니면 대화 시작
        {
            isAction = true;
            talkbox.SetActive(true);
            //talkText.gameObject.SetActive(true);
            talkText.text = obj.name + "인 것 같다.";
            
        }
        
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
