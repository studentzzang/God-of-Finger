using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuestState
{
    NotStarted,
    Accepted,
    Completed,
    Acknowledged
}

/// <summary>
/// 퀘스트 상태를 questId 기준으로 저장/조회하고, 수락/완료/제출 전이를 관리한다.
/// </summary>
public class QuestManager : Singleton<QuestManager>
{
    // 퀘스트ID, 상태 매핑 딕셔너리
    private readonly Dictionary<string, QuestState> states = new();

    // quest/questId 유효성(Null/빈 문자열) 검사
    private bool IsValidQuest(QuestSO quest)
    {
        return quest != null && !string.IsNullOrEmpty(quest.questId);
    }

    public QuestState GetState(QuestSO quest) // 퀘스트 상태 조회
    {
        if (quest == null) return QuestState.NotStarted;
        return states.TryGetValue(quest.questId, out var s) ? s : QuestState.NotStarted;
    }

    /// <summary>
    /// 퀘스트를 지금 수락할 수 있는지 확인한다.
    /// - 아직 시작 전(NotStarted)이어야 함
    /// - 선행 퀘스트가 있다면 Acknowledged(제출 완료) 상태여야 함
    /// </summary>
    public bool CanAccept(QuestSO quest)
    {
        if (!IsValidQuest(quest)) return false;
        if (GetState(quest) != QuestState.NotStarted) return false;

        // 선행 퀘스트가 없으면 바로 수락 가능
        if (quest.prerequisiteQuest == null) return true;

        // 선행 퀘스트는 제출(Acknowledged)까지 완료되어야 함
        return GetState(quest.prerequisiteQuest) == QuestState.Acknowledged;
    }

    // 퀘 수락 처리 -> NPC와 대화 후 호출
    public void Accept(QuestSO quest)
    {
        // 수락 가능 조건을 만족하지 않으면 무시
        if (!CanAccept(quest)) return;
        states[quest.questId] = QuestState.Accepted;
    }

    // 퀘스트 완료 처리 -> 미니게임 완료 후 호출
    public void Complete(QuestSO quest)
    {
        if (!IsValidQuest(quest)) return;
        if (GetState(quest) == QuestState.Accepted)
        {
            states[quest.questId] = QuestState.Completed;
            Debug.Log($"[Quest] Completed: {quest.questId}");
        }
            
    }

    // 퀘스트 제출 처리 -> NPC와 대화 후 호출
    // Removed as per instructions

    // 퀘스트 인정 처리 -> NPC와 대화 후 호출
    public void Acknowledge(QuestSO quest)
    {
        if (!IsValidQuest(quest)) return;
        if (GetState(quest) == QuestState.Completed)
        {
            states[quest.questId] = QuestState.Acknowledged;
            Debug.Log($"[Quest] Acknowledged: {quest.questId}");
        }
    }

    // 디버그용
    public void ResetQuest(QuestSO quest)
    {
        if (!IsValidQuest(quest)) return;
        states.Remove(quest.questId);
    }
    
    
    public IReadOnlyDictionary<string, QuestState> GetAllStates()
    {
        return states;
    }
    
    
}