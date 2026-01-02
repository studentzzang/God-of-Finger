using UnityEngine;

/// <summary>
/// 한 NPC가 여러 퀘스트(QuestEntry)를 담당할 수 있도록,
/// 현재 상태에 맞는 대화를 우선순위로 하나 선택해주는 컴포넌트.
/// </summary>
public class NPCQuestGiverMulti : MonoBehaviour
{
    [SerializeField] private QuestEntry[] questEntries;

    [Header("Fallback")]
    [Tooltip("모든 퀘스트에서 보여줄 게 없을 때(또는 후일담이 없는 Acknowledged 상태일 때) 기본 대사")]
    [SerializeField] private DialogueSO idleDialogue;
    
    
    
    private void Awake()
    {
        // 실수 방지: 단일 QuestGiver가 같이 붙어있으면 경고
        var single = GetComponent<NPCQuestGiver>();
        if (single != null)
        {
            Debug.LogWarning(
                $"[QuestGiverMulti] '{gameObject.name}'에 NPCQuestGiver가 함께 붙어 있습니다. " +
                $"멀티/단일 중 하나만 사용하세요.",
                this
            );
        }
    }

    /// <summary>
    /// 현재 이 NPC가 보여줘야 할 DialogueSO를 우선순위로 선택해 반환한다.
    /// </summary>
    public DialogueSO GetDialogue()
    {
        if (questEntries == null || questEntries.Length == 0)
            return idleDialogue;

        // 1) Completed(완료 직후 1회 대사)
        var d = PickCompletedOnce();
        if (d != null) return d;

        // 2) Accepted(진행중)
        d = PickAccepted();
        if (d != null) return d;

        // 3) NotStarted & CanAccept=true (새로 줄 퀘스트)
        d = PickNotStartedAcceptable();
        if (d != null) return d;

        // 4) NotStarted but CanAccept=false (Locked)
        d = PickLocked();
        if (d != null) return d;

        // 5) Acknowledged (후일담이 있는 경우만)
        d = PickAcknowledgedEpilogue();
        if (d != null) return d;

        // 6) 아무것도 없으면 기본 대사
        return idleDialogue;
    }

    private DialogueSO PickCompletedOnce()
    {
        foreach (var e in questEntries)
        {
            if (!IsValidEntry(e)) continue;

            if (QuestManager.Instance.GetState(e.quest) == QuestState.Completed)
            {
                // 완료 직후 1회 대사가 없으면, 이 퀘스트는 말할 게 없다고 간주하고 스킵
                return e.completedOnceDialogue;
            }
        }
        return null;
    }

    private DialogueSO PickAccepted()
    {
        foreach (var e in questEntries)
        {
            if (!IsValidEntry(e)) continue;

            if (QuestManager.Instance.GetState(e.quest) == QuestState.Accepted)
                return e.acceptedDialogue;
        }
        return null;
    }

    private DialogueSO PickNotStartedAcceptable()
    {
        foreach (var e in questEntries)
        {
            if (!IsValidEntry(e)) continue;

            if (QuestManager.Instance.GetState(e.quest) == QuestState.NotStarted &&
                QuestManager.Instance.CanAccept(e.quest))
            {
                return e.notStartedDialogue;
            }
        }
        return null;
    }

    private DialogueSO PickLocked()
    {
        foreach (var e in questEntries)
        {
            if (!IsValidEntry(e)) continue;

            if (QuestManager.Instance.GetState(e.quest) == QuestState.NotStarted &&
                !QuestManager.Instance.CanAccept(e.quest))
            {
                // lockedDialogue가 없으면 notStartedDialogue로 대체(선택)
                return e.lockedDialogue != null ? e.lockedDialogue : e.notStartedDialogue;
            }
        }
        return null;
    }

    private DialogueSO PickAcknowledgedEpilogue()
    {
        foreach (var e in questEntries)
        {
            if (!IsValidEntry(e)) continue;

            // ★ 핵심 규칙: Acknowledged는 후일담이 있을 때만 말한다
            if (QuestManager.Instance.GetState(e.quest) == QuestState.Acknowledged &&
                e.acknowledgedDialogue != null)
            {
                return e.acknowledgedDialogue;
            }
        }
        return null;
    }

    private static bool IsValidEntry(QuestEntry e)
    {
        return e != null && e.quest != null;
    }
}