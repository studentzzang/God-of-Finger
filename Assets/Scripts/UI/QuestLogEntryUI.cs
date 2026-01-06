using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 퀘스트 목록 한 줄(엔트리) UI.
/// Hover 시 툴팁 표시, 클릭 이벤트도 연결 가능.
/// </summary>
public class QuestLogEntryUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button button;

    private Action onClick;
    private Action onHover;
    private Action onExit;

    public void Bind(string title, string status, Action click, Action hover, Action exit)
    {
        if (titleText) titleText.text = title;
        if (statusText) statusText.text = status;

        onClick = click;
        onHover = hover;
        onExit = exit;

        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke());
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("[QuestEntry] Hover Enter");
        onHover?.Invoke();
    }
    public void OnPointerExit(PointerEventData eventData) => onExit?.Invoke();
    
    
}