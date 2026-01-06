using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// 미니 퀘스트 UI + 퀘스트 로그 창 UI를 관리한다.
/// QuestManager.Revision을 구독해 상태 변경 시 자동 갱신한다.
/// </summary>
public class QuestLogUIController : MonoBehaviour
{
    [Header("Mini UI (List)")]
    [SerializeField] private Transform miniContentRoot;          // Mini ScrollView Content
    [SerializeField] private QuestLogEntryUI miniEntryPrefab;     // Mini entry prefab (can reuse entryPrefab)

    [Header("Window UI")]
    [SerializeField] private GameObject windowPanel;      // QuestLogWindow
    [SerializeField] private Transform contentRoot;       // ScrollView Content
    [SerializeField] private QuestLogEntryUI entryPrefab; // QuestEntry Prefab
    [SerializeField] private QuestTooltipUI tooltip;      // TooltipPanel 스크립트

    [Header("Hotkey")]
    [SerializeField] private KeyCode toggleKey = KeyCode.J;

    // 생성한 엔트리 추적해서 Refresh 때 정리
    private readonly List<QuestLogEntryUI> spawned = new();
    private readonly List<QuestLogEntryUI> spawnedMini = new();

    private void OnEnable()
    {
        QuestManager.Instance.Revision.AddListener(OnRevisionChanged);
        RefreshAll();
    }

    private void OnDisable()
    {
        var qm = QuestManager.Instance;
        if (qm != null) qm.Revision.RemoveListener(OnRevisionChanged);
        ClearMiniEntries();
        ClearEntries();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey) && windowPanel != null)
        {
            bool open = !windowPanel.activeSelf;
            windowPanel.SetActive(open);

            if (open) RefreshWindow();
            else tooltip?.Hide();
        }
    }

    private void OnRevisionChanged(int _)
    {
        RefreshAll();
    }

    private void RefreshAll()
    {
        RefreshMini();
        if (windowPanel != null && windowPanel.activeSelf)
            RefreshWindow();
    }

    // 1) 미니 UI: (옵션) 스크롤 가능한 리스트 또는 1개 요약
    private void RefreshMini()
    {
        var states = QuestManager.Instance.GetAllStates();

        // (A) 미니 리스트 UI가 세팅되어 있으면: 여러 퀘스트를 전부 표시 (스크롤은 UI ScrollView로 처리)
        if (miniContentRoot != null && miniEntryPrefab != null)
        {
            ClearMiniEntries();

            var filtered = states
                .Where(kv => kv.Value == QuestState.Accepted || kv.Value == QuestState.Completed)
                .OrderBy(kv => kv.Value == QuestState.Accepted ? 0 : 1)
                .ThenBy(kv =>
                {
                    var q = QuestManager.Instance.FindQuest(kv.Key);
                    return q != null ? q.title : kv.Key;
                });

            foreach (var kv in filtered)
            {
                string questId = kv.Key;
                QuestState state = kv.Value;

                var quest = QuestManager.Instance.FindQuest(questId);
                string title = quest != null && !string.IsNullOrEmpty(quest.title) ? quest.title : questId;
                string desc = quest != null ? quest.Description : "";

                var entry = Instantiate(miniEntryPrefab, miniContentRoot, false);
                spawnedMini.Add(entry);

                entry.Bind(
                    title,
                    state == QuestState.Accepted ? "진행 중" : "완료 (제출 전)",
                    click: () => { /* 미니 UI 클릭 동작은 필요 시 확장 */ },
                    hover: () => tooltip?.Show(title, desc),
                    exit: () => tooltip?.Hide()
                );
            }

            return;
        }
    }

    // 2) 로그 창: Accepted/Completed만 표시 (Acknowledged는 숨김)
    private void RefreshWindow()
    {
        if (contentRoot == null || entryPrefab == null) return;

        ClearEntries();

        var states = QuestManager.Instance.GetAllStates();

        var filtered = states
            .Where(kv => kv.Value == QuestState.Accepted || kv.Value == QuestState.Completed)
            .OrderBy(kv => kv.Value == QuestState.Accepted ? 0 : 1)
            .ThenBy(kv =>
            {
                var q = QuestManager.Instance.FindQuest(kv.Key);
                return q != null ? q.title : kv.Key;
            });

        foreach (var kv in filtered)
        {
            string questId = kv.Key;
            QuestState state = kv.Value;

            var quest = QuestManager.Instance.FindQuest(questId);
            string title = quest != null && !string.IsNullOrEmpty(quest.title) ? quest.title : questId;
            string desc = quest != null ? quest.Description : "";

            var entry = Instantiate(entryPrefab, contentRoot,false);
            spawned.Add(entry);

            entry.Bind(
                title,
                state == QuestState.Accepted ? "진행 중" : "완료 (제출 전)",
                click: () => { /* 선택 강조/핀 고정 등 확장 가능 */ },
                hover: () => tooltip?.Show(title, desc),
                exit: () => tooltip?.Hide()
            );
        }
    }

    private void ClearEntries()
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            if (spawned[i] != null) Destroy(spawned[i].gameObject);
        }
        spawned.Clear();
    }

    private void ClearMiniEntries()
    {
        for (int i = 0; i < spawnedMini.Count; i++)
        {
            if (spawnedMini[i] != null) Destroy(spawnedMini[i].gameObject);
        }
        spawnedMini.Clear();
    }
}