using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 대화 UI의 공통 베이스.
/// - VisualRoot(이미지/배경/패널 묶음) on/off
/// - 화자/대사 텍스트 세팅
/// - 버튼 표시/라벨 세팅
///
/// Normal/Cinematic UI는 이 클래스를 상속해서
/// 자신만의 추가 요소(초상화/배경/스탠딩 등)를 확장한다.
/// </summary>
public abstract class DialogueUIBase : MonoBehaviour
{
    [Header("Base UI")]
    [Tooltip("DialogueUIROOT 아래에 있는 비주얼 루트(이미지/배경/패널 전체 묶음). Show/Hide는 이것만 토글함.")]
    [SerializeField] protected GameObject visualRoot;

    [Tooltip("텍스트/버튼이 들어있는 실제 패널(보통 visualRoot 자식).")]
    [SerializeField] protected GameObject panel;

    [SerializeField] protected TextMeshProUGUI speakerText;
    [SerializeField] protected TextMeshProUGUI lineText;

    [Header("Typewriter")]
    [SerializeField] private bool useTypewriter = true;
    [SerializeField] private float charsPerSecond = 40f;

    private Coroutine typingCo;
    private string fullLine;
    public bool IsTyping { get; private set; }

    [Header("Buttons")]
    [SerializeField] protected Button nextButton;
    [SerializeField] protected Button acceptButton;
    [SerializeField] protected Button rejectButton;

    [Header("Button Labels (TMP)")]
    [SerializeField] protected TextMeshProUGUI nextButtonText;
    [SerializeField] protected TextMeshProUGUI acceptButtonText;
    [SerializeField] protected TextMeshProUGUI rejectButtonText;

    public bool IsVisible =>
        (panel != null && panel.activeSelf) || (visualRoot != null && visualRoot.activeSelf);

    /// <summary>
    /// 패널(비주얼 루트)을 켠다.
    /// </summary>
    public virtual void Show()
    {
        // IMPORTANT: 이 컴포넌트가 붙은 DialogueUIROOT는 끄지 않는다.
        if (visualRoot) visualRoot.SetActive(true);
        if (panel) panel.SetActive(true);
    }

    /// <summary>
    /// 패널(비주얼 루트)을 끈다.
    /// </summary>
    public virtual void Hide()
    {
        StopTypingInternal();

        if (visualRoot) visualRoot.SetActive(false);
        if (panel) panel.SetActive(false);
    }

    /// <summary>
    /// 화자 이름을 세팅한다.
    /// </summary>
    public virtual void SetSpeaker(string name)
    {
        if (speakerText) speakerText.text = name ?? string.Empty;
    }

    /// <summary>
    /// 대사 텍스트를 세팅한다.
    /// </summary>
    public virtual void SetLine(string text)
    {
        fullLine = text ?? string.Empty;

        if (!useTypewriter || lineText == null)
        {
            StopTypingInternal();
            if (lineText) lineText.text = fullLine;
            return;
        }

        StartTyping(fullLine);
    }

    /// <summary>
    /// 버튼 표시 상태를 세팅한다.
    /// </summary>
    public virtual void SetButtons(bool showNext, bool showAccept, bool showReject)
    {
        if (nextButton) nextButton.gameObject.SetActive(showNext);
        if (acceptButton) acceptButton.gameObject.SetActive(showAccept);
        if (rejectButton) rejectButton.gameObject.SetActive(showReject);
    }

    /// <summary>
    /// 버튼 라벨을 세팅한다. (null이면 기존 라벨 유지)
    /// </summary>
    public virtual void SetButtonLabels(string next, string accept, string reject)
    {
        if (next != null && nextButtonText) nextButtonText.text = next;
        if (accept != null && acceptButtonText) acceptButtonText.text = accept;
        if (reject != null && rejectButtonText) rejectButtonText.text = reject;
    }

    public Button NextButton => nextButton;
    public Button AcceptButton => acceptButton;
    public Button RejectButton => rejectButton;

    public GameObject Panel => panel;

    private void StartTyping(string text)
    {
        StopTypingInternal();

        // 비활성 상태면 코루틴 시작 불가 → 즉시 출력
        if (!isActiveAndEnabled || panel == null || !panel.activeInHierarchy)
        {
            if (lineText) lineText.text = text ?? string.Empty;
            IsTyping = false;
            typingCo = null;
            return;
        }

        typingCo = StartCoroutine(TypeRoutine(text));
    }

    private IEnumerator TypeRoutine(string text)
    {
        IsTyping = true;
        lineText.text = "";

        float t = 0f;
        int i = 0;
        while (i < text.Length)
        {
            t += Time.unscaledDeltaTime * charsPerSecond;
            int next = Mathf.FloorToInt(t);
            if (next > i)
            {
                i = Mathf.Min(next, text.Length);
                lineText.text = text.Substring(0, i);
            }
            yield return null;
        }

        lineText.text = text;
        IsTyping = false;
        typingCo = null;
    }

    public void SkipTyping()
    {
        if (lineText == null) return;
        StopTypingInternal();
        lineText.text = fullLine;
        IsTyping = false;
    }

    private void StopTypingInternal()
    {
        if (typingCo != null)
        {
            StopCoroutine(typingCo);
            typingCo = null;
        }
        IsTyping = false;
    }

    public virtual void ApplyVisual(DialogueVisual visual) { }

    // Lifecycle-based binding support
    protected virtual void Awake()
    {
        StartCoroutine(BindWhenReady());
    }

    private IEnumerator BindWhenReady()
    {
        while (DialogueManager.Instance == null)
            yield return null;

        BindToManager();
    }

    protected abstract void BindToManager();
    protected abstract void UnbindFromManager();

    protected virtual void OnDestroy()
    {
        if (DialogueManager.Instance != null)
            UnbindFromManager();
    }
}