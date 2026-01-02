using UnityEngine;

[CreateAssetMenu(menuName = "GameData/Quest", fileName = "Quest")]
public class QuestSO : ScriptableObject
{
    [Header("퀘스트 기본 정보")]
    public string questId;     // "Q001"
    public string title; //"퀘스트 제목"
    [Header("선행 퀘스트 (Optional)")]
    public QuestSO prerequisiteQuest;

    [Header("퀘스트 설명")]
    [TextArea(2, 6)]
    public string Description;
}