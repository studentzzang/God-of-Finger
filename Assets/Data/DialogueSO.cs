using UnityEngine;

public enum SpeakerType
{
    Player,
    NPC
}

public enum QuestActionType
{
    None, // 기본값
    Accept, // 퀘스트 수락
    Acknowledge // 퀘스트 제출(완료 인정)
}


[System.Serializable]
public class DialogueLine
{
    [Header("Quest Action (Optional)")]
    public QuestSO quest;
    public QuestActionType questAction = QuestActionType.None;
    
    
    public SpeakerType speaker;

    // speaker가 NPC일 때만 사용 (줄마다 다른 NPC 가능)
    // NPC 대사가 아닌 특정 상황(나레이션) 시 NPC로 설정 후 공백 선언.
    public string npcName;

    [TextArea(2, 4)]
    public string text;
}

[CreateAssetMenu(menuName = "GameData/Dialogue", fileName = "Dialogue")]
public class DialogueSO : ScriptableObject
{
    [Header("Dialogue Lines (Group Dialogue)")]
    public DialogueLine[] lines;

    [Header("Choice")]
    public bool hasChoice = false;

    [Header("Choice Prompt")]
    public DialogueLine choicePromptLine; // 선택지 질문(화자/이름/텍스트) - 비워두면 기본 문구로 대체

    [Header("Choice Result Lines")]
    public DialogueLine acceptResult;
    public DialogueLine rejectResult;

    [Header("Choice Button Labels")]
    public string acceptLabel;    // 예: "수락한다"
    public string rejectLabel;    // 예: "거절한다"
    
}