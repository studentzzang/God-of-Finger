using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// 미니 퀘스트 UI + 퀘스트 로그 창 UI를 관리한다.
/// QuestManager.Revision을 구독해 상태 변경 시 자동 갱신한다.
/// </summary>
public class QuestLogUIController : MonoBehaviour
{
    [Header("Mini UI (List)")]
    [SerializeField] private Transform miniContentRoot;          // Mini 퀘스트 창
    [SerializeField] private QuestLogEntryUI miniEntryPrefab;     // Mini entry prefab

    [Header("Window UI")]
    [SerializeField] private GameObject windowPanel;      // 퀘스트 로그 창 패널
    [SerializeField] private Transform contentRoot;       // 퀘스트 로그 창 콘텐츠 루트
    [SerializeField] private QuestLogEntryUI entryPrefab; // 퀘스트 엔트리 프리펩
    [SerializeField] private QuestDetailsUI detailsUI; // 선택된 퀘스트 상세 표시

    [Header("Hotkey")]
    [SerializeField] private KeyCode toggleKey = KeyCode.J;

    // 생성한 엔트리 추적해서 Refresh 때 정리
    private readonly List<QuestLogEntryUI> spawned = new();
    private readonly List<QuestLogEntryUI> spawnedMini = new();
    private string selectedQuestId;

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

    // 퀘스트 로그 창을 연다(이미 열려있으면 유지)
    private void OpenWindow()
    {
        if (windowPanel == null) return;
        if (!windowPanel.activeSelf)
            windowPanel.SetActive(true);
    }

    // 특정 퀘스트를 선택하고 상세 패널을 갱신한다.
    private void SelectQuest(string questId)
    {
        if (string.IsNullOrEmpty(questId)) return;

        selectedQuestId = questId;

        var quest = QuestManager.Instance.FindQuest(questId);
        var state = QuestManager.Instance.GetState(questId);

        string title = quest != null && !string.IsNullOrEmpty(quest.title) ? quest.title : questId;
        string desc = quest != null ? quest.Description : "";
        string status = state == QuestState.Accepted ? "진행 중" : "완료 (제출 전)";

        detailsUI?.Show(title, status, desc);
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
                //string desc = quest != null ? quest.Description : "";

                var entry = Instantiate(miniEntryPrefab, miniContentRoot, false);
                spawnedMini.Add(entry);

                entry.Bind(
                    title,
                    state == QuestState.Accepted ? "진행 중" : "완료 (제출 전)",
                    click: () =>
                    {
                        OpenWindow();
                        RefreshWindow();
                        SelectQuest(questId);
                    }

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
            //string desc = quest != null ? quest.Description : "";

            var entry = Instantiate(entryPrefab, contentRoot, false);
            spawned.Add(entry);

            entry.Bind(
                title,
                state == QuestState.Accepted ? "진행 중" : "완료 (제출 전)",
                click: () => SelectQuest(questId)
            );
        }

        // 갱신 후 선택 유지(가능하면 기존 선택 유지, 없으면 첫 번째 자동 선택)
        if (!string.IsNullOrEmpty(selectedQuestId) && states.ContainsKey(selectedQuestId))
        {
            SelectQuest(selectedQuestId);
        }
        else
        {
            string first = filtered.Select(kv => kv.Key).FirstOrDefault();
            if (!string.IsNullOrEmpty(first)) SelectQuest(first);
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