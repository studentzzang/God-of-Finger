using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : Singleton<DialogueManager>
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI lineText;

    [Header("Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button rejectButton;

    [Header("Button TMP")]
    [SerializeField] private TextMeshProUGUI nextButtonText;
    [SerializeField] private TextMeshProUGUI acceptButtonText;
    [SerializeField] private TextMeshProUGUI rejectButtonText;

    [Header("Names / Defaults")]
    [SerializeField] private string playerName = "나";
    [SerializeField] private string defaultNpcName = " ";

    [SerializeField] private string defaultChoicePrompt = "어떻게 할까?";
    [SerializeField] private string defaultAcceptText = "알겠다.";
    [SerializeField] private string defaultRejectText = "어쩔 수 없지.";

    [Header("Default Button Labels")]
    [SerializeField] private string nextLabel = "다음";
    [SerializeField] private string closeLabel = "닫기";
    [SerializeField] private string acceptDefaultLabel = "수락";
    [SerializeField] private string rejectDefaultLabel = "거절";

    public bool IsOpen => panel != null && panel.activeSelf;

    private DialogueSO currentDialogue;
    private int lineIndex;

    private bool choiceDone;
    private bool showingChoiceResult;

    protected override void Awake()
    {
        base.Awake();

        if (nextButton) nextButton.onClick.AddListener(Next);
        if (acceptButton) acceptButton.onClick.AddListener(Accept);
        if (rejectButton) rejectButton.onClick.AddListener(Reject);

        if (panel) panel.SetActive(false);
    }

    public void StartDialogue(DialogueSO dialogue)
    {
        if (dialogue == null) return;

        currentDialogue = dialogue;
        lineIndex = 0;
        choiceDone = false;
        showingChoiceResult = false;

        panel.SetActive(true);
        ShowLine();
        UpdateButtons();
    }

    public void CloseDialogue()
    {
        currentDialogue = null;
        lineIndex = 0;
        choiceDone = false;
        showingChoiceResult = false;

        panel.SetActive(false);
    }

    public void Next()
    {
        if (currentDialogue == null) return;

        bool ended = lineIndex >= currentDialogue.lines.Length;

        // 선택지 대기 중이면 Next 금지
        if (currentDialogue.hasChoice && ended && !choiceDone)
            return;

        // 선택 결과 보여준 뒤 Next → 닫기
        if (showingChoiceResult)
        {
            CloseDialogue();
            return;
        }

        // 일반 대화 끝
        if (ended && !currentDialogue.hasChoice)
        {
            CloseDialogue();
            return;
        }

        lineIndex++;
        ShowLine();
        UpdateButtons();
    }

    private void ShowLine()
    {
        if (currentDialogue == null) return;

        // 일반 대사
        if (lineIndex < currentDialogue.lines.Length)
        {
            ApplyLine(currentDialogue.lines[lineIndex]);
            return;
        }

        // 선택지 프롬프트
        if (currentDialogue.hasChoice && !choiceDone)
        {
            speakerText.text = "";
            lineText.text = string.IsNullOrEmpty(currentDialogue.choicePrompt)
                ? defaultChoicePrompt
                : currentDialogue.choicePrompt;
            return;
        }

        CloseDialogue();
    }

    private void Accept()
    {
        choiceDone = true;

        if (currentDialogue.acceptResult != null &&
            !string.IsNullOrEmpty(currentDialogue.acceptResult.text))
        {
            ApplyLine(currentDialogue.acceptResult);
        }
        else
        {
            speakerText.text = playerName;
            lineText.text = defaultAcceptText;
        }

        showingChoiceResult = true;
        UpdateButtons();
    }

    private void Reject()
    {
        choiceDone = true;

        if (currentDialogue.rejectResult != null &&
            !string.IsNullOrEmpty(currentDialogue.rejectResult.text))
        {
            ApplyLine(currentDialogue.rejectResult);
        }
        else
        {
            speakerText.text = playerName;
            lineText.text = defaultRejectText;
        }

        showingChoiceResult = true;
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        if (currentDialogue == null) return;

        bool ended = lineIndex >= currentDialogue.lines.Length;

        // 선택 결과 단계 → 닫기
        if (showingChoiceResult)
        {
            nextButton.gameObject.SetActive(true);
            acceptButton.gameObject.SetActive(false);
            rejectButton.gameObject.SetActive(false);

            if (nextButtonText) nextButtonText.text = closeLabel;
            return;
        }

        // 선택지 단계
        if (currentDialogue.hasChoice && ended && !choiceDone)
        {
            nextButton.gameObject.SetActive(false);
            acceptButton.gameObject.SetActive(true);
            rejectButton.gameObject.SetActive(true);

            //버튼 이름 변경
            if (acceptButtonText)
                acceptButtonText.text = string.IsNullOrEmpty(currentDialogue.acceptLabel)
                    ? acceptDefaultLabel
                    : currentDialogue.acceptLabel;

            if (rejectButtonText)
                rejectButtonText.text = string.IsNullOrEmpty(currentDialogue.rejectLabel)
                    ? rejectDefaultLabel
                    : currentDialogue.rejectLabel;

            return;
        }

        // 일반 진행
        nextButton.gameObject.SetActive(true);
        acceptButton.gameObject.SetActive(false);
        rejectButton.gameObject.SetActive(false);

        if (nextButtonText) nextButtonText.text = nextLabel;
    }

    private void ApplyLine(DialogueLine line)
    {
        if (line.speaker == SpeakerType.Player)
            speakerText.text = playerName;
        else
            speakerText.text = string.IsNullOrEmpty(line.npcName)
                ? defaultNpcName
                : line.npcName;

        lineText.text = line.text;
    }
}
