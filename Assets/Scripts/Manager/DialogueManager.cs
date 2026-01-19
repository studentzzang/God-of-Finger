using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum DialoguePresentationMode
{
    Normal,
    Cinematic
}


/// <summary>
/// DialogueSO를 기반으로 대화를 진행·표시하는 중앙 매니저.
/// 대사 진행, 선택지 처리, UI 버튼 제어를 담당한다.
/// </summary>
public class DialogueManager : Singleton<DialogueManager>
{
    [Header("UI (bind from scene)")]
    private NormalDialogueUI normalUI;
    private CinematicDialogueUI cinematicUI;

    // 현재 표시/입력을 담당하는 UI
    private DialogueUIBase ui;

    // 현재 연출 모드
    private DialoguePresentationMode mode = DialoguePresentationMode.Normal;

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

    public bool IsOpen => ui != null && ui.gameObject.activeInHierarchy && ui.Panel != null && ui.Panel.activeInHierarchy;
    
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

    public void BindNormalUI(NormalDialogueUI newUI)
    {
        normalUI = newUI;
        TryAutoSelectUI();
    }

    public void BindCinematicUI(CinematicDialogueUI newUI)
    {
        cinematicUI = newUI;
        TryAutoSelectUI();
    }

    private void TryAutoSelectUI()
    {
        // 현재 모드에 맞는 UI 우선, 없으면 있는 쪽으로
        DialogueUIBase desired = mode == DialoguePresentationMode.Cinematic ? cinematicUI : normalUI;
        if (desired == null) desired = normalUI != null ? normalUI : cinematicUI;

        SetActiveUI(desired);
    }

    private void SetActiveUI(DialogueUIBase newActive)
    {
        // Hide any previously active UI (root-level hide, not Panel toggles)
        if (ui != null && ui != newActive)
        {
            ui.Hide();
        }

        ui = newActive;

        // Always hide the non-active UI as well (prevents leftover background/portrait, etc.)
        if (normalUI != null && normalUI != ui) normalUI.Hide();
        if (cinematicUI != null && cinematicUI != ui) cinematicUI.Hide();

        if (ui == null) return;

        // Button bindings (prevent duplicates)
        if (ui.NextButton)
        {
            ui.NextButton.onClick.RemoveAllListeners();
            ui.NextButton.onClick.AddListener(Next);
        }

        if (ui.AcceptButton)
        {
            ui.AcceptButton.onClick.RemoveAllListeners();
            ui.AcceptButton.onClick.AddListener(Accept);
        }

        if (ui.RejectButton)
        {
            ui.RejectButton.onClick.RemoveAllListeners();
            ui.RejectButton.onClick.AddListener(Reject);
        }

        ClearUISelection();

        // Sync current dialogue state to the active UI
        if (currentDialogue != null)
        {
            ui.Show();
            ShowLine();
            UpdateButtons();
        }
        else
        {
            ui.Hide();
        }
    }

    private void EnterCinematic()
    {
        if (mode == DialoguePresentationMode.Cinematic) return;
        mode = DialoguePresentationMode.Cinematic;
        SetActiveUI(cinematicUI != null ? cinematicUI : ui);
    }

    private void EnterNormal()
    {
        if (mode == DialoguePresentationMode.Normal) return;
        mode = DialoguePresentationMode.Normal;
        SetActiveUI(normalUI != null ? normalUI : ui);
    }

    public void ForceCinematicMode()
    {
        EnterCinematic();
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

        // 대화 시작은 항상 Normal 모드
        mode = DialoguePresentationMode.Normal;
        TryAutoSelectUI();

        // (Panel activation handled by SetActiveUI via ui.Show())

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

        mode = DialoguePresentationMode.Normal;

        // 대화 종료 시 UI 포커스 해제
        ClearUISelection();

        // Hide both UIs to ensure background/portraits/etc are fully cleared
        if (normalUI != null) normalUI.Hide();
        if (cinematicUI != null) cinematicUI.Hide();

        ui = null;
    }

    /// <summary>
    /// 다음 대사로 진행하거나, 상황에 따라 대화를 종료한다.
    /// 선택지 대기 중에는 진행을 제한한다.
    /// </summary>
    public void Next()
    {
        if (ui != null && ui.IsTyping)
        {
            ui.SkipTyping();
            return;
        }
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

        // 선택지 프롬프트 표시 (이 타이밍부터 Cinematic UI로 전환)
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
                if (ui != null)
                {
                    ui.SetSpeaker("");
                    ui.SetLine(defaultChoicePrompt);
                }
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
        if (currentDialogue == null) return;
        if (!currentDialogue.hasChoice) return;
        if (ui == null) return;
        if (ConsumeAdvanceThisFrame()) return;

        // 클릭 중 currentDialogue가 바뀌어도 안전하도록 로컬 복사
        var dialogue = currentDialogue;

        choiceDone = true;

        // acceptResult가 있으면 그 대사를 보여주고, 해당 줄에 달린 퀘스트 액션을 실행
        if (HasText(dialogue.acceptResult))
        {
            ApplyLine(dialogue.acceptResult);
            ExecuteQuestAction(dialogue.acceptResult);
        }
        else
        {
            ui.SetSpeaker(playerName);
            ui.SetLine(defaultAcceptText);
        }

        showingChoiceResult = true;
        if (ui != null) UpdateButtons();
    }

    /// <summary>
    /// 선택지에서 '거절'을 눌렀을 때 호출된다.
    /// 거절 결과 대사를 표시하고 종료 단계로 전환한다.
    /// </summary>
    private void Reject()
    {
        if (currentDialogue == null) return;
        if (!currentDialogue.hasChoice) return;
        if (ui == null) return;
        if (ConsumeAdvanceThisFrame()) return;

        // 클릭 중 currentDialogue가 바뀌어도 안전하도록 로컬 복사
        var dialogue = currentDialogue;

        choiceDone = true;

        // rejectResult가 있으면 그 대사를 보여주고, 해당 줄에 달린 퀘스트 액션을 실행
        if (HasText(dialogue.rejectResult))
        {
            ApplyLine(dialogue.rejectResult);
            ExecuteQuestAction(dialogue.rejectResult);
        }
        else
        {
            ui.SetSpeaker(playerName);
            ui.SetLine(defaultRejectText);
        }

        showingChoiceResult = true;
        if (ui != null) UpdateButtons();
    }

    /// <summary>
    /// 현재 대화 상태(일반/선택지/종료)에 맞게 버튼 가시성과 라벨을 갱신한다.
    /// </summary>
    private void UpdateButtons()
    {
        if (currentDialogue == null) return;
        if (ui == null) return;

        // 버튼 상태가 바뀔 때마다 UI 포커스를 비워 Submit(스페이스/엔터) 자동 클릭을 방지
        ClearUISelection();

        bool ended = lineIndex >= currentDialogue.lines.Length;

        // 선택지 단계(대사 라인 끝) → 모드와 무관하게 수락/거절 버튼 표시
        if (currentDialogue.hasChoice && ended && !choiceDone)
        {
            ui.SetButtons(showNext: false, showAccept: true, showReject: true);

            var acceptLabel = string.IsNullOrEmpty(currentDialogue.acceptLabel) ? acceptDefaultLabel : currentDialogue.acceptLabel;
            var rejectLabel = string.IsNullOrEmpty(currentDialogue.rejectLabel) ? rejectDefaultLabel : currentDialogue.rejectLabel;
            ui.SetButtonLabels(null, acceptLabel, rejectLabel);

            return;
        }

        // 선택 결과 단계 → 닫기 버튼만 표시
        if (showingChoiceResult)
        {
            ui.SetButtons(showNext: true, showAccept: false, showReject: false);
            ui.SetButtonLabels(closeLabel, null, null);
            return;
        }

        // 일반 진행 단계 → 다음 버튼만 표시
        ui.SetButtons(showNext: true, showAccept: false, showReject: false);
        ui.SetButtonLabels(nextLabel, null, null);
    }

    /// <summary>
    /// 전달된 DialogueLine을 UI에 적용한다.
    /// 화자에 따라 이름과 대사를 설정한다.
    /// </summary>
    private void ApplyLine(DialogueLine line)
    {
        if (line == null) return;

        // 연출 모드 전환 (라인 단위)
        bool wantCinematic = line.visual != null && line.visual.useCinematic;
        if (wantCinematic) EnterCinematic();
        else EnterNormal();

        // 플레이어 화자 처리
        if (line.speaker == SpeakerType.Player)
        {
            if (ui != null) ui.SetSpeaker(playerName);
        }
        else
        {
            // NPC 화자 처리
            if (ui != null) ui.SetSpeaker(string.IsNullOrEmpty(line.npcName) ? defaultNpcName : line.npcName);
        }

        // 대사 텍스트 적용
        if (ui != null) ui.SetLine(line.text);

        // 완료 직후 1회 대사에서만 상태 전환(Completed -> Acknowledged)
        // (Accept는 선택지 결과에서만 실행되도록 유지)
        if (line.quest != null && line.questAction == QuestActionType.Acknowledge)
        {
            if (QuestManager.Instance != null)
                QuestManager.Instance.Acknowledge(line.quest);
            else
                Debug.LogWarning("[DialogueManager] QuestManager.Instance is null (Acknowledge skipped)");
        }
    }

    // DialogueLine에 달린 퀘스트 액션을 실행한다 (없으면 아무 것도 하지 않음)
    private void ExecuteQuestAction(DialogueLine line)
    {
        if (line == null) return;
        if (line.quest == null) return;

        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("[DialogueManager] QuestManager.Instance is null (QuestAction skipped)");
            return;
        }

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