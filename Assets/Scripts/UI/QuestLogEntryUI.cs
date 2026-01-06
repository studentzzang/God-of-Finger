using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 퀘스트 목록 한 줄(엔트리) UI.
/// 클릭 시 선택/상세 보기 로직을 호출한다.
/// </summary>
public class QuestLogEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button button;

    private Action onClick;

    /// <summary>
    /// 엔트리 UI에 텍스트를 적용하고 클릭 콜백을 바인딩한다.
    /// </summary>
    public void Bind(string title, string status, Action click)
    {
        if (titleText) titleText.text = title;
        if (statusText) statusText.text = status;

        onClick = click;

        if (button)
        {
            // 버튼 이벤트가 중복 등록되는 걸 방지
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}