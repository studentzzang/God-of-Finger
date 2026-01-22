using UnityEngine;

public static class DialogueValidator
{
    public static void Validate(DialogueSO dialogue)
    {
        if (dialogue == null) return;

        // ---------- Lines ----------
        if (dialogue.lines == null || dialogue.lines.Length == 0)
        {
            Debug.LogWarning($"[DialogueValidator] Dialogue has no lines: {dialogue.name}");
        }

        foreach (var line in dialogue.lines)
        {
            ValidateLine(line, dialogue.name);
        }

        // ---------- Choice ----------
        if (dialogue.hasChoice)
        {
            if (dialogue.choicePromptLine == null)
                Debug.LogWarning($"[DialogueValidator] hasChoice=true but choicePromptLine is null: {dialogue.name}");

            if (dialogue.acceptResult == null)
                Debug.LogWarning($"[DialogueValidator] hasChoice=true but acceptResult is null: {dialogue.name}");

            if (dialogue.rejectResult == null)
                Debug.LogWarning($"[DialogueValidator] hasChoice=true but rejectResult is null: {dialogue.name}");
        }
    }

    private static void ValidateLine(DialogueLine line, string dialogueName)
    {
        if (line == null)
        {
            Debug.LogWarning($"[DialogueValidator] Null DialogueLine in {dialogueName}");
            return;
        }

        if (string.IsNullOrEmpty(line.text))
            Debug.LogWarning($"[DialogueValidator] Empty text in {dialogueName}");

        if (line.speaker == SpeakerType.NPC && string.IsNullOrEmpty(line.npcName))
            Debug.LogWarning($"[DialogueValidator] NPC line without npcName in {dialogueName}");

        if (line.visual != null)
        {
            if (line.visual.useCinematic && line.visual.portrait == null && line.visual.background == null)
                Debug.LogWarning($"[DialogueValidator] Cinematic line has no visual assets in {dialogueName}");
        }

        if (line.quest != null && line.questAction == QuestActionType.None)
            Debug.LogWarning($"[DialogueValidator] Quest assigned but questAction=None in {dialogueName}");
    }
}