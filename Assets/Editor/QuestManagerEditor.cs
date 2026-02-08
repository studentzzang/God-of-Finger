#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QuestManager))]
public class QuestManagerEditor : Editor
{
    private string questId = "";
    private Vector2 scroll;
    private bool showAllStates = true;

    public override void OnInspectorGUI()
    {
        // 기존 인스펙터(데이터베이스, enablePersistence 등) 그대로
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Quest Debug", EditorStyles.boldLabel);

        var qm = (QuestManager)target;

        // Play Mode에서만 안전하게 조작하도록 제한(원하면 풀어도 됨)
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            questId = EditorGUILayout.TextField("Quest Id", questId);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Accept")) DoAccept(qm);
                if (GUILayout.Button("Complete")) DoComplete(qm);
                if (GUILayout.Button("Acknowledge")) DoAcknowledge(qm);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Reset(NotStarted)")) DoReset(qm);
                if (GUILayout.Button("Print State")) DoPrint(qm);
            }

            EditorGUILayout.Space(6);

            // 현재 상태 표시
            if (!string.IsNullOrEmpty(questId))
            {
                var cur = qm.GetState(questId);
                EditorGUILayout.HelpBox($"Current State: {cur}", MessageType.Info);
            }

            EditorGUILayout.Space(8);

            // 전체 상태 보기(옵션)
            showAllStates = EditorGUILayout.Foldout(showAllStates, "All Quest States (runtime)");
            if (showAllStates)
            {
                DrawAllStates(qm);
            }
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to use Quest Debug buttons.", MessageType.None);
        }
    }

    private void DoAccept(QuestManager qm)
    {
        if (!ValidateId()) return;
        qm.Accept(questId);
        Debug.Log($"[QuestDebug] Accept({questId}) -> {qm.GetState(questId)}");
    }

    private void DoComplete(QuestManager qm)
    {
        if (!ValidateId()) return;
        qm.Complete(questId);
        Debug.Log($"[QuestDebug] Complete({questId}) -> {qm.GetState(questId)}");
    }

    private void DoAcknowledge(QuestManager qm)
    {
        if (!ValidateId()) return;
        qm.Acknowledge(questId);
        Debug.Log($"[QuestDebug] Acknowledge({questId}) -> {qm.GetState(questId)}");
    }

    private void DoReset(QuestManager qm)
    {
        if (!ValidateId()) return;

        // ResetQuest는 QuestSO를 받으므로, DB에 있으면 그걸로 리셋.
        var quest = qm.FindQuest(questId);
        if (quest != null)
        {
            qm.ResetQuest(quest);
            Debug.Log($"[QuestDebug] ResetQuest({questId}) -> {qm.GetState(questId)}");
            return;
        }

        // DB에 없으면 현재 API로는 states.Remove(questId)를 직접 할 수 없으니 안내만.
        Debug.LogWarning($"[QuestDebug] '{questId}' is not in QuestDatabaseSO. ResetQuest requires QuestSO. (DB에 추가하거나 Reset(string) API를 추가하면 됨)");
    }

    private void DoPrint(QuestManager qm)
    {
        if (!ValidateId()) return;
        Debug.Log($"[QuestDebug] State({questId}) = {qm.GetState(questId)}");
    }

    private bool ValidateId()
    {
        if (string.IsNullOrEmpty(questId))
        {
            Debug.LogWarning("[QuestDebug] questId is empty.");
            return false;
        }
        return true;
    }

    private void DrawAllStates(QuestManager qm)
    {
        var dict = qm.GetAllStates();
        if (dict == null || dict.Count == 0)
        {
            EditorGUILayout.HelpBox("No quest states yet. (Empty)", MessageType.Info);
            return;
        }

        // 간단 스크롤 리스트
        int maxRows = 20;
        float rowHeight = EditorGUIUtility.singleLineHeight + 2;

        using (var sv = new EditorGUILayout.ScrollViewScope(scroll, GUILayout.Height(Mathf.Min(dict.Count, maxRows) * rowHeight + 10)))
        {
            scroll = sv.scrollPosition;

            foreach (var kv in dict)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(kv.Key, GUILayout.MinWidth(140));
                    EditorGUILayout.LabelField(kv.Value.ToString(), GUILayout.Width(110));

                    // 클릭하면 questId에 채우기
                    if (GUILayout.Button("Use", GUILayout.Width(50)))
                    {
                        questId = kv.Key;
                        GUI.FocusControl(null);
                    }
                }
            }
        }
    }
}
#endif