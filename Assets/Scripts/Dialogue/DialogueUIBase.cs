using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 대화 UI의 공통 베이스.
/// - 패널 on/off
/// - 화자/대사 텍스트 세팅
/// - 버튼 표시/라벨 세팅
/// 
/// Normal/Cinematic UI는 이 클래스를 상속해서
/// 자신만의 추가 요소(초상화/배경/스탠딩 등)를 확장한다.
/// </summary>
public abstract class DialogueUIBase : MonoBehaviour
{
    [Header("Base UI")]
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

    public bool IsVisible => panel != null && panel.activeSelf;

    /// <summary>
    /// 패널을 켠다.
    /// </summary>
    public virtual void Show()
    {
        gameObject.SetActive(true);
        if (panel) panel.SetActive(true);
    }

    /// <summary>
    /// 패널을 끈다.
    /// </summary>
    public virtual void Hide()
    {
        gameObject.SetActive(false);
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

    /// <summary>
    /// DialogueManager가 버튼 이벤트를 연결할 수 있게 버튼 참조를 제공한다.
    /// </summary>
    public Button NextButton => nextButton;
    public Button AcceptButton => acceptButton;
    public Button RejectButton => rejectButton;

    public GameObject Panel => panel;

    private void StartTyping(string text)
    {
        StopTypingInternal();
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
        // Wait until DialogueManager.Instance is not null
        while (DialogueManager.Instance == null)
        {
            yield return null;
        }
        BindToManager();
    }

    protected abstract void BindToManager();
    protected abstract void UnbindFromManager();

    protected virtual void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            UnbindFromManager();
        }
    }
}   