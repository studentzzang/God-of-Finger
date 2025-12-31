using UnityEngine;

public enum SpeakerType
{
    Player,
    NPC
}

[System.Serializable]
public class DialogueLine
{
    public SpeakerType speaker;

    // speaker가 NPC일 때만 의미 있음 (NPC1/NPC2 등 줄마다 지정)
    public string npcName;
    
    [TextArea(2, 4)]
    public string text;
}

[CreateAssetMenu(menuName = "GameData/Dialogue", fileName = "Dialogue")]
public class DialogueSO : ScriptableObject
{
    [Header("Lines (Group Dialogue)")]
    public DialogueLine[] lines;

    [Header("Optional Choice (Accept/Reject)")]
    public bool hasChoice = false;

    [TextArea(2, 4)]
    public string acceptResultLine;

    [TextArea(2, 4)]
    public string rejectResultLine;
}