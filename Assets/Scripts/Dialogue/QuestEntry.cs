using UnityEngine;

/// <summary>
/// 퀘스트 1개에 대한 상태별 대사 세트(데이터 묶음).
/// NPCQuestGiverMulti가 이 묶음들을 여러 개 들고 우선순위로 선택한다.
/// </summary>
[System.Serializable]
public class QuestEntry
{
    [Header("Quest")]
    public QuestSO quest;

    [Header("Dialogues by State")]
    public DialogueSO notStartedDialogue;      // 아직 수락 전
    public DialogueSO acceptedDialogue;        // 진행 중(수락 후)

    [Tooltip("퀘스트 조건 달성(Completed) 직후, 1회만 보여줄 대사")]
    public DialogueSO completedOnceDialogue;   // 완료 직후 1회

    [Tooltip("완료 1회 대사 이후(Acknowledged) 반복 대사. 마지막 퀘스트에만 넣어야 함.")]
    public DialogueSO acknowledgedDialogue;    // 후일담(선택)

    [Header("Locked (Optional)")]
    [Tooltip("선행퀘 미충족 등으로 아직 수락 불가일 때 보여줄 대사(선택)")]
    public DialogueSO lockedDialogue;
}