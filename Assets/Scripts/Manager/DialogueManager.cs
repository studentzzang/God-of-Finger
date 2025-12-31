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

    [Header("Names / System Text")]
    [SerializeField] private string playerName = "나";
    [SerializeField] private string choicePromptText = "어떻게 할까?";
    [SerializeField] private string defaultAcceptText = "알겠다.";
    [SerializeField] private string defaultRejectText = "어쩔 수 없지.";

    public bool IsOpen => panel != null && panel.activeSelf;

    private DialogueSO currentDialogue;
    private int lineIndex;
    private bool choiceDone;

    protected override void Awake()
    {
        base.Awake();

        // 버튼 클릭 연결(Inspector OnClick 비워둬도 됨)
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

        panel.SetActive(true);
        ShowLine();
        UpdateButtons();
    }

    public void CloseDialogue()
    {
        currentDialogue = null;
        lineIndex = 0;
        choiceDone = false;

        if (panel) panel.SetActive(false);
    }

    public void Next()
    {
        if (currentDialogue == null) return;

        bool ended = lineIndex >= currentDialogue.lines.Length;

        // 선택지 대화: 끝났고 아직 선택 안 했으면 Next 막기
        if (currentDialogue.hasChoice && ended && !choiceDone)
            return;

        // 끝났고 선택지도 없으면 닫기
        if (ended && !currentDialogue.hasChoice)
        {
            CloseDialogue();
            return;
        }

        // 다음 줄로
        lineIndex++;
        ShowLine();
        UpdateButtons();
    }

    private void ShowLine()
    {
        if (currentDialogue == null) return;

        // 아직 대사가 남아있으면 출력
        if (lineIndex < currentDialogue.lines.Length)
        {
            var line = currentDialogue.lines[lineIndex];

            // speakerText 결정
            if (line.speaker == SpeakerType.Player)
            {
                speakerText.text = playerName;
            }
            else
            {
                // NPC 줄인데 npcName 비어있으면 "NPC" 같은 기본값으로
                speakerText.text = string.IsNullOrEmpty(line.npcName) ? "NPC" : line.npcName;
            }

            lineText.text = line.text;
            return;
        }

        // 대사가 끝났을 때
        if (currentDialogue.hasChoice && !choiceDone)
        {
            speakerText.text = "";
            lineText.text = choicePromptText;
        }
        else
        {
            CloseDialogue(); // "(대화 끝)" 표시 없이 자동 종료
        }
    }

    private void UpdateButtons()
    {
        if (currentDialogue == null) return;

        bool ended = lineIndex >= currentDialogue.lines.Length;

        // 선택지 단계
        if (currentDialogue.hasChoice && ended && !choiceDone)
        {
            nextButton.gameObject.SetActive(false);
            acceptButton.gameObject.SetActive(true);
            rejectButton.gameObject.SetActive(true);
            return;
        }

        // 일반 진행
        nextButton.gameObject.SetActive(true);
        acceptButton.gameObject.SetActive(false);
        rejectButton.gameObject.SetActive(false);
    }

    private void Accept()
    {
        if (currentDialogue == null) return;

        choiceDone = true;
        acceptButton.gameObject.SetActive(false);
        rejectButton.gameObject.SetActive(false);

        // 결과는 보통 플레이어 대사로 처리(취향)
        speakerText.text = playerName;
        lineText.text = string.IsNullOrEmpty(currentDialogue.acceptResultLine)
            ? defaultAcceptText
            : currentDialogue.acceptResultLine;

        nextButton.gameObject.SetActive(true);
    }

    private void Reject()
    {
        if (currentDialogue == null) return;

        choiceDone = true;
        acceptButton.gameObject.SetActive(false);
        rejectButton.gameObject.SetActive(false);

        speakerText.text = playerName;
        lineText.text = string.IsNullOrEmpty(currentDialogue.rejectResultLine)
            ? defaultRejectText
            : currentDialogue.rejectResultLine;

        nextButton.gameObject.SetActive(true);
    }
}
