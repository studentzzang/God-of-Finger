using UnityEngine;

/// <summary>
/// 퀘스트 디버그용 단축키.
/// - 1: Accept
/// - 2: Complete
/// - 3: Acknowledge
/// </summary>
public class QuestDebugKeys : MonoBehaviour
{
    [SerializeField] private QuestSO testQuest;

    private void Update()
    {
        if (testQuest == null) return;

        // 1) 수락
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            QuestManager.Instance.Accept(testQuest);
            Debug.Log($"[TEST] Accept: {testQuest.questId}");
        }

        // 2) 완료 처리(조건 달성)
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            QuestManager.Instance.Complete(testQuest);
            Debug.Log($"[TEST] Complete: {testQuest.questId}");
        }

        // 3) 완료 1회 대사 이후 단계로 넘김
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            QuestManager.Instance.Acknowledge(testQuest);
            Debug.Log($"[TEST] Acknowledge: {testQuest.questId}");
        }
    }
}