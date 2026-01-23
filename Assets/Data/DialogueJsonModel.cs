[System.Serializable]
public class DialogueJson
{
    public DialogueLineJson[] lines;

    public bool hasChoice;

    public DialogueLineJson choicePromptLine;
    public DialogueLineJson[] acceptLines;
    public DialogueLineJson[] rejectLines;

    public string acceptLabel;
    public string rejectLabel;
}

[System.Serializable]
public class DialogueLineJson
{
    public string speaker;     // "Player" / "NPC"
    public string npcName;
    public string text;

    // 연출 (줄 단위)
    public string portrait;    // "Portraits/NPC_01"
    public string background;  // "Backgrounds/Shop" 
    public string presentation; // "Normal" / "Cinematic" / "FullCinematic"
    
    
    // 퀘스트 (줄 단위)
    public string questId;      // QuestSO.questId
    public string questAction;  // "Accept" / "Acknowledge" / "None"
}  