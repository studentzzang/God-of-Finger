using System;
using UnityEngine;

[Serializable]
public class QuestDatabaseJson
{
    public QuestJson[] quests;
}

[Serializable]
public class QuestJson
{
    public string questId;
    public string title;
    public string description;

    public string prerequisiteQuestId; // optional
    public string completeSignalId;     // optional
}

public static class QuestJsonParser
{
    public static QuestDatabaseJson Parse(TextAsset jsonAsset)
    {
        if (jsonAsset == null)
        {
            Debug.LogError("[QuestJsonParser] jsonAsset is null");
            return null;
        }
        return Parse(jsonAsset.text);
    }

    public static QuestDatabaseJson Parse(string jsonText)
    {
        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError("[QuestJsonParser] jsonText is null/empty");
            return null;
        }

        var data = JsonUtility.FromJson<QuestDatabaseJson>(jsonText);
        if (data == null)
        {
            Debug.LogError("[QuestJsonParser] Invalid JSON (FromJson returned null)");
            return null;
        }

        if (data.quests == null) data.quests = Array.Empty<QuestJson>();
        return data;
    }
}