using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// 미니 퀘스트 UI + 퀘스트 로그 창 UI를 관리한다.
/// - QuestManager.Revision을 구독해 상태 변경 시 자동 갱신한다.
/// - Bootstrap/QuestManager 로드 순서가 늦어도 안전하게 대기 후 바인딩한다.
/// - OnEnable/OnDisable 반복에서도 중복 구독이 발생하지 않게 보호한다.
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
    [SerializeField] private QuestDetailsUI detailsUI;    // 선택된 퀘스트 상세 표시

    [Header("Hotkey")]
    [SerializeField] private KeyCode toggleKey = KeyCode.J;

    // 생성한 엔트리 추적해서 Refresh 때 정리
    private readonly List<QuestLogEntryUI> spawned = new();
    private readonly List<QuestLogEntryUI> spawnedMini = new();
    private string selectedQuestId;

    // 바인딩 상태/루틴
    private Coroutine bindRoutine;
    private bool isBound;

    private void OnEnable()
    {
        // 혹시라도 중복 호출되면 방지
        if (bindRoutine != null) StopCoroutine(bindRoutine);
        bindRoutine = StartCoroutine(BindWhenReady());
    }

    private void OnDisable()
    {
        if (bindRoutine != null)
        {
            StopCoroutine(bindRoutine);
            bindRoutine = null;
        }

        Unbind();
        ClearMiniEntries();
        ClearEntries();
    }

    private IEnumerator BindWhenReady()
    {
        // QuestManager 생성/초기화 타이밍이 늦어도 안전하게 대기
        while (QuestManager.Instance == null)
            yield return null;

        Bind();
        RefreshAll();
    }

    private void Bind()
    {
        if (isBound) return;

        var qm = QuestManager.Instance;
        if (qm == null) return;

        qm.Revision.AddListener(OnRevisionChanged);
        isBound = true;
    }

    private void Unbind()
    {
        if (!isBound) return;

        var qm = QuestManager.Instance;
        if (qm != null)
            qm.Revision.RemoveListener(OnRevisionChanged);

        isBound = false;
    }

    private void Update()
    {
        if (!isBound) return;

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
        if (!isBound) return;

        RefreshMini();
        if (windowPanel != null && windowPanel.activeSelf)
            RefreshWindow();
    }

    private void OpenWindow()
    {
        if (windowPanel == null) return;
        if (!windowPanel.activeSelf)
            windowPanel.SetActive(true);
    }

    private void SelectQuest(string questId)
    {
        if (!isBound) return;
        if (string.IsNullOrEmpty(questId)) return;

        selectedQuestId = questId;

        var quest = QuestManager.Instance.FindQuest(questId);
        var state = QuestManager.Instance.GetState(questId);

        string title = quest != null && !string.IsNullOrEmpty(quest.title) ? quest.title : questId;
        string desc = quest != null ? quest.Description : "";
        string status = state == QuestState.Accepted ? "진행 중" : "완료 (제출 전)";

        if (detailsUI != null)
            detailsUI.Show(title, status, desc);
    }

    private void RefreshMini()
    {
        if (!isBound) return;

        var states = QuestManager.Instance.GetAllStates();

        // 미니 리스트 UI가 세팅되어 있으면: 여러 퀘스트를 전부 표시
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

    private void RefreshWindow()
    {
        if (!isBound) return;
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
            })
            .ToList();

        foreach (var kv in filtered)
        {
            string questId = kv.Key;
            QuestState state = kv.Value;

            var quest = QuestManager.Instance.FindQuest(questId);
            string title = quest != null && !string.IsNullOrEmpty(quest.title) ? quest.title : questId;

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