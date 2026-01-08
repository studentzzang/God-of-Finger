using UnityEngine;

public class NPCQuestGiver : MonoBehaviour
{
    [Header("Quest")]
    public QuestSO quest;

    [Header("Dialogues by State")]
    public DialogueSO notStartedDialogue;
    public DialogueSO acceptedDialogue;

    [Tooltip("퀘스트 조건을 달성(Completed)한 직후 1회만 보여줄 대화")]
    public DialogueSO completedOnceDialogue;

    [Tooltip("완료 1회 대사 이후(ACKNOWLEDGED) 반복해서 보여줄 대화")]
    public DialogueSO acknowledgedDialogue;

    [Header("Locked (Optional)")]
    public DialogueSO lockedDialogue; // 아직 수락 불가(선행 조건 미충족)일 때 보여줄 대화

    public DialogueSO GetDialogue()
    {
        // 아직 시작 전인데 수락 조건이 안 되면 잠금 대화를 우선 반환
        if (QuestManager.Instance.GetState(quest) == QuestState.NotStarted &&
            !QuestManager.Instance.CanAccept(quest))
        {
            return lockedDialogue != null ? lockedDialogue : notStartedDialogue;
        }

        var state = QuestManager.Instance.GetState(quest);

        switch (state)
        {
            case QuestState.NotStarted:
                return notStartedDialogue;

            case QuestState.Accepted:
                return acceptedDialogue;

            case QuestState.Completed:
                // 완료 직후 1회 대사(없으면 바로 반복 대사로 대체)
                return completedOnceDialogue != null ? completedOnceDialogue : acknowledgedDialogue;

            case QuestState.Acknowledged:
                return acknowledgedDialogue != null ? acknowledgedDialogue : completedOnceDialogue;

            default:
                return notStartedDialogue;
        }
    }
}