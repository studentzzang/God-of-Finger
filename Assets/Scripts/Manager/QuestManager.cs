using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.IO;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;

/// <summary>
/// 퀘스트 상태 열거형
/// </summary>

public enum QuestState
{
    NotStarted, // 수락 전 
    Accepted, //수락 상태 
    Completed, // 완료 상태
    Acknowledged // 제출(인정) 상태
}



/// <summary>
/// 퀘스트 상태를 questId 기준으로 저장/조회하고, 수락/완료/제출 전이를 관리한다.
/// </summary>
public class QuestManager : Singleton<QuestManager>
{
    [Header("Database")]
    [SerializeField] private QuestDatabaseSO database;

    // UI 갱신 트리거(값 자체 의미 없음)
    public Observable<int> Revision = new Observable<int>(0);

    // 퀘스트ID, 상태 매핑 딕셔너리
    private readonly Dictionary<string, QuestState> states = new();
    
    [SerializeField] private bool enablePersistence = false;
    
    
    
    
    [System.Serializable]
    private class QuestStateEntry
    {
        public string questId;
        public QuestState state;
    }

    [System.Serializable]
    private class QuestSaveData
    {
        public List<QuestStateEntry> entries = new List<QuestStateEntry>();
    }
    
    private string SavePath => Path.Combine(Application.persistentDataPath, "quests.json");
    protected override void Awake()
    {
        base.Awake();

        // 데이터베이스가 지정되어 있다면 lookup 테이블을 구성
        if (database != null)
            database.Build();
        else
        {
            Debug.LogWarning("DataBaseSO가 할당되지 않았습니다. QuestManager가 정상 동작하지 않을 수 있습니다.");
        }
        QuestSignals.OnSignal += HandleSignal;
        //DeleteSave();
        //Load();
    }
    
    protected override void OnDestroy()
    {
        QuestSignals.OnSignal -= HandleSignal;
        base.OnDestroy(); // 싱글톤 정리(Instance=null 같은거) 보장
    }
    

    // quest/questId 유효성(Null/빈 문자열) 검사
    private bool IsValidQuest(QuestSO quest)
    {
        return quest != null && !string.IsNullOrEmpty(quest.questId);
    }

    private bool IsValidQuestId(string questId)
    {
        return !string.IsNullOrEmpty(questId);
    }

    /// <summary>
    /// questId로 QuestSO를 찾는다. (DB가 없거나 못 찾으면 null)
    /// </summary>
    public QuestSO FindQuest(string questId)
    {
        return database != null ? database.Find(questId) : null;
    }

    public QuestState GetState(QuestSO quest) // 퀘스트 상태 조회
    {
        if (!IsValidQuest(quest)) return QuestState.NotStarted;
        return GetState(quest.questId);
    }

    /// <summary>
    /// questId로 퀘스트 상태 조회
    /// </summary>
    public QuestState GetState(string questId)
    {
        if (!IsValidQuestId(questId)) return QuestState.NotStarted;
        return states.TryGetValue(questId, out var s) ? s : QuestState.NotStarted;
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
        BumpRevision();
        //Save();
    }

    public void Accept(string questId)
    {
        if (!IsValidQuestId(questId)) return;

        var quest = FindQuest(questId);
        if (quest != null)
        {
            Accept(quest);
            return;
        }
        Save();

        // DB에 없으면 선행퀘 검사 없이 단순 수락(테스트/디버그용)
        if (GetState(questId) != QuestState.NotStarted) return;
        states[questId] = QuestState.Accepted;
        BumpRevision();
    }

    // 퀘스트 완료 처리 -> 미니게임 완료 후 호출
    public void Complete(QuestSO quest)
    {
        if (!IsValidQuest(quest)) return;
        if (GetState(quest) == QuestState.Accepted)
        {
            states[quest.questId] = QuestState.Completed;
            BumpRevision();
            Debug.Log($"[Quest] Completed: {quest.questId}");
        }

        Save();
    }

    public void Complete(string questId)
    {
        if (!IsValidQuestId(questId)) return;
        if (GetState(questId) != QuestState.Accepted) return;

        states[questId] = QuestState.Completed;
        BumpRevision();
        Save();
        Debug.Log($"[Quest] Completed: {questId}");
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
            BumpRevision();
            Debug.Log($"[Quest] Acknowledged: {quest.questId}");
        }

        Save();
    }

    public void Acknowledge(string questId)
    {
        if (!IsValidQuestId(questId)) return;
        if (GetState(questId) != QuestState.Completed) return;

        states[questId] = QuestState.Acknowledged;
        BumpRevision();
        Save();
        Debug.Log($"[Quest] Acknowledged: {questId}");
    }

    // 디버그용
    public void ResetQuest(QuestSO quest)
    {
        if (!IsValidQuest(quest)) return;
        if (states.Remove(quest.questId))
            BumpRevision();
        Save();
    }
    
    
    public IReadOnlyDictionary<string, QuestState> GetAllStates()
    {
        return states;
    }

    // Revision은 "변경 신호"만 필요하므로 안전하게 증가
    private void BumpRevision()
    {
        int v = Revision.Value;
        Revision.Value = (v == int.MaxValue) ? 0 : v + 1;
    }

    private void HandleSignal(string signal)
    {
        // DB 없으면 questId -> QuestSO를 못 찾아서 자동완료가 어려움
        if (database == null) return;

        // Accepted 상태인 퀘스트 중 signalId가 일치하는 걸 완료 처리
        foreach (var kv in states.ToArray())
        {
            if (kv.Value != QuestState.Accepted) continue;

            var quest = FindQuest(kv.Key);
            if (quest == null) continue;

            if (!string.IsNullOrEmpty(quest.completeSignalId) &&
                quest.completeSignalId == signal)
            {
                Complete(kv.Key); 
            }
        }
        
    }


    public void Save()
    {
        if (!enablePersistence) return;
        try
        {
            QuestSaveData data = new QuestSaveData();
            foreach (var kv in states)
            {
                data.entries.Add(new QuestStateEntry { questId = kv.Key, state = kv.Value });

            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[QuestManager] Quests saved to {SavePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void Load()
    {
        if (!enablePersistence) return;
        
        try
        {
            if (!File.Exists(SavePath)){
                Debug.Log("no save file found");
                return;
            }
            
            string json = File.ReadAllText(SavePath);
            QuestSaveData data = JsonUtility.FromJson<QuestSaveData>(json);
            states.Clear();
            if (data!=null && data.entries!=null)
            {
                foreach (var entry in data.entries)
                {
                    if (!string.IsNullOrEmpty(entry.questId))
                        states[entry.questId] = entry.state;
                }
            }
            BumpRevision();
            Debug.Log($"[QuestManager] Quests loaded from {SavePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }

        
    }
    
    public void DeleteSave()
    {
        try
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);

            Debug.Log("[Quest] Save deleted.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Quest] DeleteSave failed: {e}");
        }
    }
}