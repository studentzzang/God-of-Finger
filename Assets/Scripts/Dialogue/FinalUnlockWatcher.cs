using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalUnlockWatcher : MonoBehaviour
{
    [Header("Milestone Dialogues (all dialogues including final should be configured here)")]
    [Tooltip("예: 2개/4개 같은 중간 알림 대화. acknowledgedCount가 현재 완료(제출) 개수 이하가 되면 1회 재생됩니다.")]
    [SerializeField] private List<MilestoneDialogue> milestones = new List<MilestoneDialogue>
    {
    };

    [Header("Playback")]
    [SerializeField] private int delayFrames = 2;

    private bool pending;
    private bool playedFallback;

    private QuestManager boundQuestManager;

    // 코루틴에서 이번에 재생할 대화(마일스톤)
    private DialogueSO pendingDialogue;
    private string pendingOnceKey;

    [Serializable]
    public class MilestoneDialogue
    {
        [Min(0)] public int acknowledgedCount;
        public DialogueSO dialogue;
        public string onceKey;
    }

    private void Awake()
    {
        // Sort once so we always evaluate milestones from the smallest count.
        if (milestones != null)
            milestones.Sort((a, b) => a.acknowledgedCount.CompareTo(b.acknowledgedCount));
    }

    private void OnEnable()
    {
        TryBindQuestManager();
        TrySchedule("OnEnable");
    }

    private void OnDisable()
    {
        UnbindQuestManager();
    }

    private void Update()
    {
        if (QuestManager.Instance != boundQuestManager)
            TryBindQuestManager();
    }

    private void TryBindQuestManager()
    {
        var qm = QuestManager.Instance;
        if (qm == boundQuestManager) return;

        UnbindQuestManager();
        boundQuestManager = qm;

        if (boundQuestManager != null)
        {
            boundQuestManager.Revision.AddListener(OnRevisionChanged);
        }
        else
        {
            Debug.LogWarning("[FinalUnlockWatcher] QuestManager.Instance is null");
        }
    }

    private void UnbindQuestManager()
    {
        if (boundQuestManager != null)
            boundQuestManager.Revision.RemoveListener(OnRevisionChanged);

        boundQuestManager = null;
    }

    private void OnRevisionChanged(int _)
    {
        TrySchedule("Revision");
    }

    private void TrySchedule(string from)
    {
        if (pending) return;

        // RuntimeOnceFlags가 없는 프로젝트/씬이라면, 런타임 전체 1회만 보장
        if (RuntimeOnceFlags.Instance == null && playedFallback)
            return;

        int acknowledged = CountAcknowledged();

        Debug.Log($"[FinalUnlockWatcher] TrySchedule from={from} acknowledged={acknowledged} isOpen={(DialogueManager.Instance != null && DialogueManager.Instance.IsOpen)} milestones={(milestones == null ? 0 : milestones.Count)}");

        // 모든 알림/해금 대화는 milestones로만 관리한다.
        if (TryPickMilestoneDialogue(acknowledged, out var milestoneDialogue, out var milestoneKey))
        {
            pendingDialogue = milestoneDialogue;
            pendingOnceKey = milestoneKey;
            pending = true;
            StartCoroutine(CoPlayPendingDialogue());
        }
    }

    private bool TryPickMilestoneDialogue(int acknowledged, out DialogueSO dialogue, out string key)
    {
        dialogue = null;
        key = null;

        if (milestones == null || milestones.Count == 0)
            return false;

        foreach (var m in milestones)
        {
            if (m == null) continue;
            if (m.acknowledgedCount < 0) continue;
            if (m.dialogue == null) continue;
            if (string.IsNullOrEmpty(m.onceKey)) continue;

            // 이미 본 마일스톤은 스킵해서 다음 후보로 넘어간다.
            if (RuntimeOnceFlags.Instance != null && RuntimeOnceFlags.Instance.HasShown(m.onceKey))
                continue;

            if (acknowledged < m.acknowledgedCount)
                continue;

            dialogue = m.dialogue;
            key = m.onceKey;
            return true;
        }

        return false;
    }

    private int CountAcknowledged()
    {
        var qm = QuestManager.Instance;
        if (qm == null) return 0;

        int count = 0;
        foreach (var kv in qm.GetAllStates())
        {
            if (kv.Value >= QuestState.Acknowledged)
                count++;
        }
        return count;
    }

    private IEnumerator CoPlayPendingDialogue()
    {
        for (int i = 0; i < delayFrames; i++)
            yield return null;

        var dialogueToPlay = pendingDialogue;
        var keyToMark = pendingOnceKey;

        if (dialogueToPlay == null)
        {
            pending = false;
            yield break;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("[FinalUnlockWatcher] DialogueManager.Instance is null.");
            pending = false;
            yield break;
        }

        while (DialogueManager.Instance.IsOpen)
            yield return null;

        
        if (RuntimeOnceFlags.Instance != null)
        {
            if (!RuntimeOnceFlags.Instance.TryMarkShown(keyToMark))
            {
                // 이미 본 마일스톤이면 다음 후보를 찾기 위해 다음 프레임에 재평가
                pending = false;
                pendingDialogue = null;
                pendingOnceKey = null;
                yield return null;
                TrySchedule("AlreadyShown");
                yield break;
            }
        }
        else
        {
            if (playedFallback)
            {
                pending = false;
                yield break;
            }
            playedFallback = true;
        }

        Debug.Log($"[FinalUnlockWatcher] Play dialogue key={keyToMark}");
        DialogueManager.Instance.StartDialogue(dialogueToPlay);

        // 다음 마일스톤이 연속으로 조건을 만족했을 수도 있으니, 대화 종료 후 다음 프레임에 재평가
        pending = false;
        pendingDialogue = null;
        pendingOnceKey = null;

        // 대화가 시작되었으니, 닫힐 때까지는 CoPlayPendingDialogue에서 기다리진 않는다.
        // Revision 이벤트나, 다음 프레임 TrySchedule로 이어지게 하려면 아래 한 번 호출.
        yield return null;
        TrySchedule("AfterPlay");
    }
}