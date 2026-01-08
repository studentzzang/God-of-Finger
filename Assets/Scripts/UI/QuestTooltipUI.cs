using TMPro;
using UnityEngine;

/// <summary>
/// 퀘스트 설명 툴팁 UI.
/// </summary>
public class QuestTooltipUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descText;

    private void Awake()
    {
        if (panel == null) panel = gameObject;
        Hide();
    }

    public void Show(string title, string desc)
    {
        if (panel) panel.SetActive(true);
        if (titleText) titleText.text = title;
        if (descText) descText.text = string.IsNullOrEmpty(desc) ? "(설명 없음)" : desc;
    }

    public void Hide()
    {
        if (panel) panel.SetActive(false);
    }
}