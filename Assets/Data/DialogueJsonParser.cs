using UnityEngine;

/// <summary>
/// JSON(TextAsset/string) -> DialogueSO 변환 파서 (런타임/에디터 공용)
/// - JsonUtility로 DTO(DialogueJson)로 파싱
/// - DialogueSO를 CreateInstance로 생성
/// - lines/choice/quest/visual을 매핑
///
/// 주의:
/// - portrait, background는 Resources 경로 기반으로 찾는다.
///   * Sprite: Resources.Load<Sprite>(portraitPath)
///   * portraitPath 예: "Portraits/NPC_01"
///   * backgroundPath 예: "Backgrounds/Shop"
/// - cinematic은 DialogueVisual.useCinematic로 매핑
/// - quest는 QuestDatabaseSO(database.Find)에서 questId로 탐색
///   (database 미주입 시 quest는 null로 파싱되고 경고 로그가 출력된다)
/// </summary>
public static class DialogueJsonParser
{
    /// <summary>
    /// TextAsset(JSON) -> DialogueSO
    /// - database를 주입하면 questId를 QuestDatabaseSO에서 resolve 한다.
    /// - database가 null이면 questId는 무시(quest=null)되고 경고를 출력한다.
    /// </summary>
    public static DialogueSO ParseFromJson(TextAsset jsonAsset, QuestDatabaseSO database)
    {
        if (jsonAsset == null)
        {
            Debug.LogError("[DialogueParser] JSON asset is null");
            return null;
        }

        return ParseFromJson(jsonAsset.text, database);
    }

    /// <summary>
    /// TextAsset(JSON) -> DialogueSO (호환용: DB 미주입)
    /// </summary>
    public static DialogueSO ParseFromJson(TextAsset jsonAsset)
    {
        return ParseFromJson(jsonAsset, database: null);
    }

    /// <summary>
    /// JSON string -> DialogueSO
    /// - database를 주입하면 questId를 QuestDatabaseSO에서 resolve 한다.
    /// - database가 null이면 questId는 무시(quest=null)되고 경고를 출력한다.
    /// </summary>
    public static DialogueSO ParseFromJson(string jsonText, QuestDatabaseSO database)
    {
        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError("[DialogueParser] JSON text is null/empty");
            return null;
        }

        var data = JsonUtility.FromJson<DialogueJson>(jsonText);
        if (data == null)
        {
            Debug.LogError("[DialogueParser] Invalid JSON (FromJson returned null)");
            return null;
        }

        Debug.Log($"[DialogueParser] Parsed JSON: hasChoice={data.hasChoice}, lines={(data.lines == null ? 0 : data.lines.Length)}, acceptLines={(data.acceptLines == null ? 0 : data.acceptLines.Length)}, rejectLines={(data.rejectLines == null ? 0 : data.rejectLines.Length)}");

        if (database == null)
            Debug.LogWarning("[DialogueParser] QuestDatabaseSO is null. questId fields will not be resolved.");
        else
            database.Build();

        var dialogue = ScriptableObject.CreateInstance<DialogueSO>();

        // ---------- Lines ----------
        dialogue.lines = ParseLines(data.lines, database);

        // ---------- Choice ----------
        dialogue.hasChoice = data.hasChoice;
        dialogue.choicePromptLine = ParseLine(data.choicePromptLine, database);

        // Choice results: arrays (new format)
        dialogue.acceptLines = ParseLinesOrEmpty(data.acceptLines, database);
        dialogue.rejectLines = ParseLinesOrEmpty(data.rejectLines, database);

        dialogue.acceptLabel = data.acceptLabel;
        dialogue.rejectLabel = data.rejectLabel;

        if (dialogue.hasChoice)
        {
            if (dialogue.choicePromptLine == null)
                Debug.LogWarning("[DialogueParser] hasChoice=true but choicePromptLine is null (will fall back to default prompt)");

            if (dialogue.acceptLines == null || dialogue.acceptLines.Length == 0)
                Debug.LogWarning("[DialogueParser] hasChoice=true but acceptLines is empty (will fall back to default accept text)");

            if (dialogue.rejectLines == null || dialogue.rejectLines.Length == 0)
                Debug.LogWarning("[DialogueParser] hasChoice=true but rejectLines is empty (will fall back to default reject text)");
        }

        return dialogue;
    }

    /// <summary>
    /// JSON string -> DialogueSO (호환용: DB 미주입)
    /// </summary>
    public static DialogueSO ParseFromJson(string jsonText)
    {
        return ParseFromJson(jsonText, database: null);
    }

    private static DialogueLine[] ParseLines(DialogueLineJson[] jsonLines, QuestDatabaseSO database)
    {
        if (jsonLines == null) return System.Array.Empty<DialogueLine>();

        var result = new DialogueLine[jsonLines.Length];
        for (int i = 0; i < jsonLines.Length; i++)
            result[i] = ParseLine(jsonLines[i], database);

        return result;
    }

    private static DialogueLine[] ParseLinesOrEmpty(DialogueLineJson[] jsonLines, QuestDatabaseSO database)
    {
        if (jsonLines == null || jsonLines.Length == 0) return System.Array.Empty<DialogueLine>();
        return ParseLines(jsonLines, database);
    }

    private static DialogueLine ParseLine(DialogueLineJson json, QuestDatabaseSO database)
    {
        if (json == null) return null;

        var line = new DialogueLine
        {
            speaker = ParseSpeaker(json.speaker),
            npcName = json.npcName,
            text = json.text,

            quest = FindQuest(json.questId, database),
            questAction = ParseQuestAction(json.questAction),

            visual = ParseVisual(json)
        };

        return line;
    }

    // ---------- Helpers ----------

    private static SpeakerType ParseSpeaker(string value)
    {
        // 기본값 NPC (문자열 실수 대비)
        return value == "Player" ? SpeakerType.Player : SpeakerType.NPC;
    }

    private static QuestActionType ParseQuestAction(string value)
    {
        if (string.IsNullOrEmpty(value)) return QuestActionType.None;
        if (System.Enum.TryParse(value, true, out QuestActionType result)) return result;
        return QuestActionType.None;
    }

    private static QuestSO FindQuest(string questId, QuestDatabaseSO database)
    {
        if (string.IsNullOrEmpty(questId)) return null;

        if (database == null)
        {
            Debug.LogWarning($"[DialogueParser] QuestDatabaseSO is null. Cannot resolve questId: {questId}");
            return null;
        }

        var quest = database.Find(questId);
        if (quest == null)
            Debug.LogWarning($"[DialogueParser] Quest not found in QuestDatabaseSO: {questId}");

        return quest;
    }

    private static DialogueVisual ParseVisual(DialogueLineJson json)
    {
        // 둘 다 없으면 굳이 객체 만들지 않음
        if (!json.useCinematic && string.IsNullOrEmpty(json.portrait) && string.IsNullOrEmpty(json.background))
            return null;

        var visual = new DialogueVisual
        {
            useCinematic = json.useCinematic
        };

        // portrait, background는 Resources 경로 문자열
        // 예: "Portraits/NPC_01"
        if (!string.IsNullOrEmpty(json.portrait))
        {
            visual.portrait = Resources.Load<Sprite>(json.portrait);
            if (visual.portrait == null)
                Debug.LogWarning($"[DialogueParser] Portrait not found: {json.portrait} (Resources path)");
        }

        // background는 Resources 경로 문자열
        // 예: "Backgrounds/Shop"
        if (!string.IsNullOrEmpty(json.background))
        {
            visual.background = Resources.Load<Sprite>(json.background);
            if (visual.background == null)
                Debug.LogWarning($"[DialogueParser] Background not found: {json.background} (Resources path)");
        }

        return visual;
    }
}