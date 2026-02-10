using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// questId -> QuestSO lookup을 제공하는 데이터베이스.
/// UI/세이브/이벤트 연동에서 "ID로 에셋 찾기"가 필요할 때 사용한다.
/// </summary>
[CreateAssetMenu(menuName = "GameData/QuestDatabase", fileName = "QuestDatabase")]
public class QuestDatabaseSO : ScriptableObject
{
    [SerializeField] private QuestSO[] quests;

    private Dictionary<string, QuestSO> map;

    /// <summary>
    /// 내부 lookup 테이블을 구성한다. (에디터/런타임 어느 쪽에서도 호출 가능)
    /// </summary>
    public void Build()
    {
        map = new Dictionary<string, QuestSO>();

        if (quests == null) return;

        foreach (var q in quests)
        {
            if (q == null) continue;
            if (string.IsNullOrEmpty(q.questId)) continue;

            // 같은 ID 중복이면 마지막이 덮어씀(원하면 경고 로그로 바꿀 수 있음)
            map[q.questId] = q;
        }
    }

    /// <summary>
    /// questId로 QuestSO를 찾는다. 없으면 null.
    /// </summary>
    public QuestSO Find(string questId)
    {
        if (string.IsNullOrEmpty(questId)) return null;

        if (map == null) Build();

        return map.TryGetValue(questId, out var q) ? q : null;
    }

    /// <summary>
    /// (선택) UI 등에서 전체 퀘스트 목록이 필요할 때 사용.
    /// </summary>
    public IEnumerable<QuestSO> All()
    {
        return quests;
    }
}