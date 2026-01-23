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
            // choicePromptLine은 "없으면 기본 문구"로 대체하는 흐름일 수 있으니,
            // null 자체는 경고만(에러 X)로 둔다.
            if (dialogue.choicePromptLine == null)
                Debug.LogWarning($"[DialogueValidator] hasChoice=true but choicePromptLine is null (will fallback to default prompt): {dialogue.name}");

            // 신버전: 선택 결과는 단일 라인이 아니라 라인 묶음
            if (dialogue.acceptLines == null || dialogue.acceptLines.Length == 0)
                Debug.LogWarning($"[DialogueValidator] hasChoice=true but acceptLines is empty: {dialogue.name}");

            if (dialogue.rejectLines == null || dialogue.rejectLines.Length == 0)
                Debug.LogWarning($"[DialogueValidator] hasChoice=true but rejectLines is empty: {dialogue.name}");

            // 결과 라인들도 라인 단위 검증 수행
            if (dialogue.choicePromptLine != null)
                ValidateLine(dialogue.choicePromptLine, dialogue.name);

            if (dialogue.acceptLines != null)
            {
                foreach (var line in dialogue.acceptLines)
                    ValidateLine(line, dialogue.name);
            }

            if (dialogue.rejectLines != null)
            {
                foreach (var line in dialogue.rejectLines)
                    ValidateLine(line, dialogue.name);
            }
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