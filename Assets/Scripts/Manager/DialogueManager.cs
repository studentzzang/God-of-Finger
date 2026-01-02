using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


/// <summary>
/// DialogueSO를 기반으로 대화를 진행·표시하는 중앙 매니저.
/// 대사 진행, 선택지 처리, UI 버튼 제어를 담당한다.
/// </summary>
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
    
    public bool IsWaitingChoice =>
        currentDialogue != null &&
        currentDialogue.hasChoice &&
        lineIndex >= currentDialogue.lines.Length &&
        !choiceDone &&
        !showingChoiceResult;

    private DialogueSO currentDialogue;
    private int lineIndex;

    private bool choiceDone;
    private bool showingChoiceResult;

    // 같은 프레임에 Next/Accept/Reject가 중복 호출되는 것을 방지
    private int lastAdvanceFrame = -1;

    protected override void Awake()
    {
        base.Awake();

        DisableButtonNavigation(nextButton);
        DisableButtonNavigation(acceptButton);
        DisableButtonNavigation(rejectButton);

        // 버튼 이벤트가 중복 등록되는 걸 방지
        if (nextButton)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(Next);
        }

        if (acceptButton)
        {
            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(Accept);
        }

        if (rejectButton)
        {
            rejectButton.onClick.RemoveAllListeners();
            rejectButton.onClick.AddListener(Reject);
        }

        ClearUISelection();

        if (panel) panel.SetActive(false);
    }

    /// <summary>
    /// 지정된 DialogueSO로 대화를 시작한다.
    /// 내부 상태를 초기화하고 첫 줄을 표시한다.
    /// </summary>
    public void StartDialogue(DialogueSO dialogue)
    {
        if (dialogue == null) return;

        currentDialogue = dialogue;
        lineIndex = 0;
        choiceDone = false;
        showingChoiceResult = false;

        if (panel) panel.SetActive(true);

        // 대화 시작 시 UI 포커스 해제(Submit 방지)
        ClearUISelection();

        ShowLine();
        UpdateButtons();
    }

    /// <summary>
    /// 현재 대화를 종료하고 UI를 닫는다.
    /// </summary>
    public void CloseDialogue()
    {
        currentDialogue = null;
        lineIndex = 0;
        choiceDone = false;
        showingChoiceResult = false;

        // 대화 종료 시 UI 포커스 해제
        ClearUISelection();

        if (panel) panel.SetActive(false);
    }

    /// <summary>
    /// 다음 대사로 진행하거나, 상황에 따라 대화를 종료한다.
    /// 선택지 대기 중에는 진행을 제한한다.
    /// </summary>
    public void Next()
    {
        if (currentDialogue == null) return;
        if (ConsumeAdvanceThisFrame()) return;

        bool ended = lineIndex >= currentDialogue.lines.Length;

        // 선택지 대기 중이면 Next(진행) 입력을 막는다
        if (currentDialogue.hasChoice && ended && !choiceDone)
            return;

        // 선택 결과를 보여준 상태라면 Next로 대화를 닫는다
        if (showingChoiceResult)
        {
            CloseDialogue();
            return;
        }

        // 선택지가 없는 일반 대화가 끝났으면 닫는다
        if (ended && !currentDialogue.hasChoice)
        {
            CloseDialogue();
            return;
        }

        // 다음 줄로 진행
        lineIndex++;
        ShowLine();
        UpdateButtons();
    }

    /// <summary>
    /// 현재 인덱스에 맞는 대사 또는 선택지 프롬프트를 화면에 표시한다.
    /// </summary>
    private void ShowLine()
    {
        if (currentDialogue == null) return;

        // 일반 대사 표시
        if (lineIndex < currentDialogue.lines.Length)
        {
            ApplyLine(currentDialogue.lines[lineIndex]);
            return;
        }

        // 선택지 프롬프트 표시
        if (currentDialogue.hasChoice && !choiceDone)
        {
            // choicePromptLine이 있으면(텍스트가 있으면) 화자/이름까지 포함해서 표시
            if (HasText(currentDialogue.choicePromptLine))
            {
                ApplyLine(currentDialogue.choicePromptLine);
            }
            else
            {
                // 없으면 기본 문구로 표시(화자 없음)
                speakerText.text = "";
                lineText.text = defaultChoicePrompt;
            }

            return;
        }

        // 종료 처리
        CloseDialogue();
    }

    /// <summary>
    /// 선택지에서 '수락'을 눌렀을 때 호출된다.
    /// 수락 결과 대사를 표시하고 종료 단계로 전환한다.
    /// </summary>
    private void Accept()
    {
        if (ConsumeAdvanceThisFrame()) return;

        choiceDone = true;

        // acceptResult가 있으면 그 대사를 보여주고, 해당 줄에 달린 퀘스트 액션을 실행
        if (HasText(currentDialogue.acceptResult))
        {
            ApplyLine(currentDialogue.acceptResult);
            ExecuteQuestAction(currentDialogue.acceptResult);
        }
        else
        {
            speakerText.text = playerName;
            lineText.text = defaultAcceptText;
        }

        showingChoiceResult = true;
        UpdateButtons();
    }

    /// <summary>
    /// 선택지에서 '거절'을 눌렀을 때 호출된다.
    /// 거절 결과 대사를 표시하고 종료 단계로 전환한다.
    /// </summary>
    private void Reject()
    {
        if (ConsumeAdvanceThisFrame()) return;

        choiceDone = true;

        // rejectResult가 있으면 그 대사를 보여주고, 해당 줄에 달린 퀘스트 액션을 실행
        if (HasText(currentDialogue.rejectResult))
        {
            ApplyLine(currentDialogue.rejectResult);
            ExecuteQuestAction(currentDialogue.rejectResult);
        }
        else
        {
            speakerText.text = playerName;
            lineText.text = defaultRejectText;
        }

        showingChoiceResult = true;
        UpdateButtons();
    }

    /// <summary>
    /// 현재 대화 상태(일반/선택지/종료)에 맞게 버튼 가시성과 라벨을 갱신한다.
    /// </summary>
    private void UpdateButtons()
    {
        if (currentDialogue == null) return;

        // 버튼 상태가 바뀔 때마다 UI 포커스를 비워 Submit(스페이스/엔터) 자동 클릭을 방지
        ClearUISelection();

        bool ended = lineIndex >= currentDialogue.lines.Length;

        // 선택 결과 단계 → 닫기 버튼만 표시
        if (showingChoiceResult)
        {
            if (nextButton) nextButton.gameObject.SetActive(true);
            if (acceptButton) acceptButton.gameObject.SetActive(false);
            if (rejectButton) rejectButton.gameObject.SetActive(false);

            if (nextButtonText) nextButtonText.text = closeLabel;
            return;
        }

        // 선택지 단계 → 수락/거절 버튼 표시
        if (currentDialogue.hasChoice && ended && !choiceDone)
        {
            if (nextButton) nextButton.gameObject.SetActive(false);
            if (acceptButton) acceptButton.gameObject.SetActive(true);
            if (rejectButton) rejectButton.gameObject.SetActive(true);

            ClearUISelection();

            // 버튼 이름 변경
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

        // 일반 진행 단계 → 다음 버튼만 표시
        if (nextButton) nextButton.gameObject.SetActive(true);
        if (acceptButton) acceptButton.gameObject.SetActive(false);
        if (rejectButton) rejectButton.gameObject.SetActive(false);

        if (nextButtonText) nextButtonText.text = nextLabel;
    }

    /// <summary>
    /// 전달된 DialogueLine을 UI에 적용한다.
    /// 화자에 따라 이름과 대사를 설정한다.
    /// </summary>
    private void ApplyLine(DialogueLine line)
    {
        if (line == null) return;

        // 플레이어 화자 처리
        if (line.speaker == SpeakerType.Player)
        {
            speakerText.text = playerName;
        }
        else
        {
            // NPC 화자 처리
            speakerText.text = string.IsNullOrEmpty(line.npcName)
                ? defaultNpcName
                : line.npcName;
        }

        // 대사 텍스트 적용
        lineText.text = line.text;

        // 완료 직후 1회 대사에서만 상태 전환(Completed -> Acknowledged)
        // (Accept는 선택지 결과에서만 실행되도록 유지)
        if (line.quest != null && line.questAction == QuestActionType.Acknowledge)
        {
            QuestManager.Instance.Acknowledge(line.quest);
        }
    }

    // DialogueLine에 달린 퀘스트 액션을 실행한다 (없으면 아무 것도 하지 않음)
    private void ExecuteQuestAction(DialogueLine line)
    {
        if (line == null) return;
        if (line.quest == null) return;

        switch (line.questAction)
        {
            case QuestActionType.Accept:
                QuestManager.Instance.Accept(line.quest);
                break;
            case QuestActionType.Acknowledge:
                QuestManager.Instance.Acknowledge(line.quest);
                break;
            case QuestActionType.None:
            default:
                break;
        }
    }

    // 대사가 유효하게 존재하는지(텍스트가 비어있지 않은지) 확인
    private static bool HasText(DialogueLine line)
    {
        return line != null && !string.IsNullOrEmpty(line.text);
    }

    /// <summary>
    /// UI 선택(포커스)을 해제하여 Space/Enter의 Submit이 버튼을 자동 클릭하지 않게 한다.
    /// </summary>
    private void ClearUISelection()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    // 같은 프레임에 여러 번 진행(스킵)되는 것을 막는다
    private bool ConsumeAdvanceThisFrame()
    {
        if (Time.frameCount == lastAdvanceFrame) return true;
        lastAdvanceFrame = Time.frameCount;
        return false;
    }

    // 키보드/패드 네비게이션으로 버튼이 선택되는 것을 막는다(선택 = Submit 대상이 될 수 있음)
    private static void DisableButtonNavigation(Button b)
    {
        if (!b) return;
        var nav = b.navigation;
        nav.mode = Navigation.Mode.None;
        b.navigation = nav;
    }
    
    /// <summary>
    /// 선택지 대기 상태에서 키보드 Y/N 입력으로 수락/거절을 처리한다.
    /// </summary>
    private void Update()
    {
        if (!IsOpen || currentDialogue == null) return;

        // 선택지 대기 중이면 Y/N만 처리
        if (IsWaitingChoice)
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                Accept();
            }
            else if (Input.GetKeyDown(KeyCode.N))
            {
                Reject();
            }
        }
    }
}