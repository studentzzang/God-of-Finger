#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class QuestJsonImporter : AssetPostprocessor
{
    // 네가 원하는 경로
    private const string JsonRoot = "Assets/GameData/QuestsJson";
    private const string OutputRoot = "Assets/GameData/Quests";

    // DB 에셋 위치(원하는 곳으로 바꿔도 됨)
    private const string DatabaseAssetPath = "Assets/GameData/QuestDatabase.asset";

    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        bool anyQuestChanged = false;

        // 1) 새로 들어오거나 수정된 JSON
        foreach (var path in importedAssets)
        {
            if (!IsQuestJson(path)) continue;
            ImportQuestJson(path);
            anyQuestChanged = true;
        }

        // 2) 이동된 JSON도 처리(새 위치)
        foreach (var path in movedAssets)
        {
            if (!IsQuestJson(path)) continue;
            ImportQuestJson(path);
            anyQuestChanged = true;
        }

        // 3) 삭제된 JSON이면 대응되는 .asset도 지움(선택)
        foreach (var path in deletedAssets)
        {
            if (!IsQuestJson(path)) continue;
            DeleteQuestAssetForJson(path);
            anyQuestChanged = true;
        }

        // 4) JSON이 바뀐 게 있으면 DB 갱신
        if (anyQuestChanged)
        {
            RefreshQuestDatabase();
            AssetDatabase.SaveAssets();
        }
    }

    private static bool IsQuestJson(string path)
    {
        return path != null
               && path.StartsWith(JsonRoot, StringComparison.OrdinalIgnoreCase)
               && path.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
    }

    private static void ImportQuestJson(string jsonPath)
    {
        EnsureFolder(OutputRoot);

        string jsonText = File.ReadAllText(jsonPath);
        var data = QuestJsonParser.Parse(jsonText);
        if (data == null)
        {
            Debug.LogError($"[QuestImporter] Parse failed: {jsonPath}");
            return;
        }

        // 1) 먼저 questId -> QuestSO 에셋을 전부 생성/갱신 (선행퀘 참조는 2-pass에서 연결)
        var createdOrUpdated = new List<QuestSO>();

        foreach (var q in data.quests ?? Array.Empty<QuestJson>())
        {
            if (q == null || string.IsNullOrEmpty(q.questId))
            {
                Debug.LogWarning($"[QuestImporter] Skipped quest with empty questId in {jsonPath}");
                continue;
            }

            string soPath = $"{OutputRoot}/{q.questId}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<QuestSO>(soPath);

            if (existing == null)
            {
                existing = ScriptableObject.CreateInstance<QuestSO>();
                existing.questId = q.questId;

                AssetDatabase.CreateAsset(existing, soPath);
                Debug.Log($"[QuestImporter] Created QuestSO: {soPath}");
            }

            // 본문 필드 갱신
            existing.title = q.title ?? "";
            existing.Description = q.description ?? "";
            existing.completeSignalId = q.completeSignalId ?? "";

            EditorUtility.SetDirty(existing);
            createdOrUpdated.Add(existing);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 2) prerequisiteQuestId 연결 (2-pass)
        // OutputRoot 안의 모든 QuestSO를 로드해서 연결하는게 안전함
        var allQuestAssets = LoadAllQuestsInOutputRoot();
        var byId = allQuestAssets
            .Where(x => x != null && !string.IsNullOrEmpty(x.questId))
            .ToDictionary(x => x.questId, x => x);

        foreach (var q in data.quests ?? Array.Empty<QuestJson>())
        {
            if (q == null || string.IsNullOrEmpty(q.questId)) continue;

            if (!byId.TryGetValue(q.questId, out var so) || so == null) continue;

            if (string.IsNullOrEmpty(q.prerequisiteQuestId))
            {
                if (so.prerequisiteQuest != null)
                {
                    so.prerequisiteQuest = null;
                    EditorUtility.SetDirty(so);
                }
                continue;
            }

            if (byId.TryGetValue(q.prerequisiteQuestId, out var pre))
            {
                if (so.prerequisiteQuest != pre)
                {
                    so.prerequisiteQuest = pre;
                    EditorUtility.SetDirty(so);
                }
            }
            else
            {
                Debug.LogWarning($"[QuestImporter] prerequisiteQuestId not found: {q.prerequisiteQuestId} (for {q.questId})");
            }
        }

        AssetDatabase.SaveAssets();
    }

    private static void DeleteQuestAssetForJson(string jsonPath)
    {
        // JSON 하나가 여러 퀘스트를 담을 수도 있어서 “삭제” 정책은 애매함.
        // 여기서는 안전하게 아무것도 안 지우는 걸 추천.
        // (정말 지우고 싶으면, json 파일 내용을 읽어서 questId 목록을 찾아 해당 asset 삭제하는 방식으로 구현해야 함)
        Debug.LogWarning($"[QuestImporter] JSON deleted: {jsonPath}. (No QuestSO deleted by default)");
    }

    private static QuestSO[] LoadAllQuestsInOutputRoot()
    {
        // OutputRoot 내부의 에셋을 모두 로드
        var guids = AssetDatabase.FindAssets("t:QuestSO", new[] { OutputRoot });
        var list = new List<QuestSO>(guids.Length);
        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            var so = AssetDatabase.LoadAssetAtPath<QuestSO>(path);
            if (so != null) list.Add(so);
        }
        return list.ToArray();
    }

    private static void RefreshQuestDatabase()
    {
        EnsureFolder(Path.GetDirectoryName(DatabaseAssetPath)?.Replace("\\", "/") ?? "Assets/GameData");

        var db = AssetDatabase.LoadAssetAtPath<QuestDatabaseSO>(DatabaseAssetPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<QuestDatabaseSO>();
            AssetDatabase.CreateAsset(db, DatabaseAssetPath);
            Debug.Log($"[QuestImporter] Created QuestDatabaseSO: {DatabaseAssetPath}");
        }

        // DB에 quests 배열 넣기 (private 필드라 SerializedObject로 강제 세팅)
        var allQuests = LoadAllQuestsInOutputRoot()
            .OrderBy(q => q.questId) // 보기 좋게 정렬(선택)
            .ToArray();

        var so = new SerializedObject(db);
        var questsProp = so.FindProperty("quests");
        if (questsProp == null)
        {
            Debug.LogError("[QuestImporter] QuestDatabaseSO has no serialized field named 'quests'");
            return;
        }

        questsProp.arraySize = allQuests.Length;
        for (int i = 0; i < allQuests.Length; i++)
        {
            questsProp.GetArrayElementAtIndex(i).objectReferenceValue = allQuests[i];
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(db);

        // 런타임/에디터 어느 쪽에서도 바로 Find가 먹게 lookup 빌드
        db.Build();

        Debug.Log($"[QuestImporter] QuestDatabase refreshed: {allQuests.Length} quests");
    }

    private static void EnsureFolder(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath)) return;

        folderPath = folderPath.Replace("\\", "/");

        if (AssetDatabase.IsValidFolder(folderPath)) return;

        // "Assets/GameData/Quests" 같은 경로를 단계적으로 생성
        string[] parts = folderPath.Split('/');
        if (parts.Length == 0) return;

        string current = parts[0]; // "Assets"
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }

        AssetDatabase.Refresh();
    }
}
#endif