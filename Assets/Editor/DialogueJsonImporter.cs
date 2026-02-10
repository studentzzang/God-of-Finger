using System;
using UnityEditor;
using UnityEngine;
using System.IO;

public class DialogueJsonImporter : AssetPostprocessor
{
    private const string JsonRoot = "Assets/GameData/DialoguesJson";
    private const string OutputRoot = "Assets/GameData/Dialogues";
    private const string QuestDatabaseAssetPath = "Assets/GameData/QuestDatabase.asset";

    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        // 1) 새로 들어오거나 수정된 JSON
        foreach (var path in importedAssets)
        {
            if (!IsDialogueJson(path)) continue;
            ImportDialogue(path);
        }

        // 2) 이동된 JSON도 처리(새 위치)
        foreach (var path in movedAssets)
        {
            if (!IsDialogueJson(path)) continue;
            ImportDialogue(path);
        }

        // (선택) 삭제 대응은 필요하면 추가. 기본은 유지.
    }

    private static bool IsDialogueJson(string path)
    {
        return !string.IsNullOrEmpty(path)
               && path.StartsWith(JsonRoot, StringComparison.OrdinalIgnoreCase)
               && path.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
    }

    private static void ImportDialogue(string jsonPath)
    {
        var jsonText = File.ReadAllText(jsonPath);

        // Quest DB를 로드해서 questId 해석에 사용
        var questDb = AssetDatabase.LoadAssetAtPath<QuestDatabaseSO>(QuestDatabaseAssetPath);

        DialogueSO dialogue;
        if (questDb != null)
        {
            // DB 기반 파싱(questId -> QuestSO 연결)
            dialogue = DialogueJsonParser.ParseFromJson(jsonText, questDb);
        }
        else
        {
            // DB가 아직 생성/갱신 전일 수 있음(임포트 순서 문제)
            // 이 경우에도 대화 SO는 생성하되, quest 연결은 null로 두고 경고만 띄움
            Debug.LogWarning($"[DialogueImporter] QuestDatabaseSO not found at '{QuestDatabaseAssetPath}'. Dialogue will be imported without quest references: {jsonPath}");
            dialogue = DialogueJsonParser.ParseFromJson(jsonText);
        }

        if (dialogue == null)
        {
            Debug.LogError($"[DialogueImporter] Parse failed: {jsonPath}");
            return;
        }

        // 파일명 기준으로 SO 이름 결정
        string fileName = Path.GetFileNameWithoutExtension(jsonPath);
        // Unity는 에셋의 메인 오브젝트 이름과 파일명이 다르면 경고를 낼 수 있음
        // (특히 ScriptableObject.CreateInstance로 만든 인스턴스의 name이 비어있을 때)
        if (dialogue != null) dialogue.name = fileName;
        string soPath = $"{OutputRoot}/{fileName}.asset";

        // 폴더 없으면 생성 (AssetsDatabase 방식으로 안전하게)
        EnsureFolder(OutputRoot);

        var existing = AssetDatabase.LoadAssetAtPath<DialogueSO>(soPath);
        if (existing == null)
        {
            AssetDatabase.CreateAsset(dialogue, soPath);
            Debug.Log($"[DialogueImporter] Created DialogueSO: {soPath}");
        }
        else
        {
            EditorUtility.CopySerialized(dialogue, existing);
            UnityEngine.Object.DestroyImmediate(dialogue);
            Debug.Log($"[DialogueImporter] Updated DialogueSO: {soPath}");
        }

        AssetDatabase.SaveAssets();
    }

    private static void EnsureFolder(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath)) return;

        folderPath = folderPath.Replace("\\", "/");

        if (AssetDatabase.IsValidFolder(folderPath)) return;

        string[] parts = folderPath.Split('/');
        if (parts.Length == 0) return;

        string current = parts[0]; // "Assets"
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }

        AssetDatabase.Refresh();
    }
}