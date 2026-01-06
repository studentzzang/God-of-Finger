using TMPro;
using UnityEngine;

/// <summary>
/// 선택된 퀘스트의 상세(제목/상태/설명)를 표시한다.
/// </summary>
public class QuestDetailsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    //[SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI descText;

    public void Show(string title, string status, string desc)
    {
        if (titleText) titleText.text = title;
        //if (statusText) statusText.text = status;
        if (descText) descText.text = string.IsNullOrEmpty(desc) ? "(설명 없음)" : desc;
    }

    public void Clear()
    {
        if (titleText) titleText.text = "";
        //if (statusText) statusText.text = "";
        if (descText) descText.text = ""; 
    }
}