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
    private DialogueUI ui;

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

    public bool IsOpen => ui != null && ui.panel != null && ui.panel.activeSelf;
    
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
    }

    public void BindUI(DialogueUI newUI)
    {
        ui = newUI;
        if (ui == null) return;

        // Button bindings (prevent duplicates)
        if (ui.nextButton)
        {
            ui.nextButton.onClick.RemoveAllListeners();
            ui.nextButton.onClick.AddListener(Next);
        }

        if (ui.acceptButton)
        {
            ui.acceptButton.onClick.RemoveAllListeners();
            ui.acceptButton.onClick.AddListener(Accept);
        }

        if (ui.rejectButton)
        {
            ui.rejectButton.onClick.RemoveAllListeners();
            ui.rejectButton.onClick.AddListener(Reject);
        }

        ClearUISelection();

        // Sync current dialogue state to the new UI
        if (currentDialogue != null)
        {
            if (ui.panel) ui.panel.SetActive(true);
            ShowLine();
            UpdateButtons();
        }
        else
        {
            if (ui.panel) ui.panel.SetActive(false);
        }
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

        if (ui?.panel) ui.panel.SetActive(true);

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

        if (ui?.panel) ui.panel.SetActive(false);
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
                if (ui?.speakerText) ui.speakerText.text = "";
                if (ui?.lineText) ui.lineText.text = defaultChoicePrompt;
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
            if (ui?.speakerText) ui.speakerText.text = playerName;
            if (ui?.lineText) ui.lineText.text = defaultAcceptText;
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
            if (ui?.speakerText) ui.speakerText.text = playerName;
            if (ui?.lineText) ui.lineText.text = defaultRejectText;
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
            if (ui?.nextButton) ui.nextButton.gameObject.SetActive(true);
            if (ui?.acceptButton) ui.acceptButton.gameObject.SetActive(false);
            if (ui?.rejectButton) ui.rejectButton.gameObject.SetActive(false);

            if (ui?.nextButtonText) ui.nextButtonText.text = closeLabel;
            return;
        }

        // 선택지 단계 → 수락/거절 버튼 표시
        if (currentDialogue.hasChoice && ended && !choiceDone)
        {
            if (ui?.nextButton) ui.nextButton.gameObject.SetActive(false);
            if (ui?.acceptButton) ui.acceptButton.gameObject.SetActive(true);
            if (ui?.rejectButton) ui.rejectButton.gameObject.SetActive(true);

            ClearUISelection();

            // 버튼 이름 변경
            if (ui?.acceptButtonText)
                ui.acceptButtonText.text = string.IsNullOrEmpty(currentDialogue.acceptLabel)
                    ? acceptDefaultLabel
                    : currentDialogue.acceptLabel;

            if (ui?.rejectButtonText)
                ui.rejectButtonText.text = string.IsNullOrEmpty(currentDialogue.rejectLabel)
                    ? rejectDefaultLabel
                    : currentDialogue.rejectLabel;

            return;
        }

        // 일반 진행 단계 → 다음 버튼만 표시
        if (ui?.nextButton) ui.nextButton.gameObject.SetActive(true);
        if (ui?.acceptButton) ui.acceptButton.gameObject.SetActive(false);
        if (ui?.rejectButton) ui.rejectButton.gameObject.SetActive(false);

        if (ui?.nextButtonText) ui.nextButtonText.text = nextLabel;
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
            if (ui?.speakerText) ui.speakerText.text = playerName;
        }
        else
        {
            // NPC 화자 처리
            if (ui?.speakerText) ui.speakerText.text = string.IsNullOrEmpty(line.npcName)
                ? defaultNpcName
                : line.npcName;
        }

        // 대사 텍스트 적용
        if (ui?.lineText) ui.lineText.text = line.text;

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