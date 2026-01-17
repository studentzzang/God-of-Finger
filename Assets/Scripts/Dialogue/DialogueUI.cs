using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 씬에 존재하는 대화 UI.
/// DialogueManager에 자신을 바인딩하고,
/// 실제 UI 표시 요소(panel, 텍스트, 버튼 등)를 제공한다.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI lineText;

    [Header("Buttons")]
    public Button nextButton;
    public Button acceptButton;
    public Button rejectButton;

    [Header("Button TMP")]
    public TextMeshProUGUI nextButtonText;
    public TextMeshProUGUI acceptButtonText;
    public TextMeshProUGUI rejectButtonText;

    private void Awake()    
    {
        // 씬에 UI가 생성될 때 자동으로 매니저에 등록
        DialogueManager.Instance.BindUI(this);
    }

    private void OnDestroy()
    {
        // 씬이 언로드되며 UI가 사라질 때 자동 해제
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.BindUI(null);
    }
}