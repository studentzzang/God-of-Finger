using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorToScene : MonoBehaviour
{
    [Header("Destination")]
    [SerializeField] private SceneName targetScene;
    [SerializeField] private string targetSpawnPointId = "Default";

    [Header("Quest Gate")]
    [SerializeField] private bool useQuestGate = false;
    [SerializeField] private string requiredQuestId = "";

    public enum GateRule
    {
        AcceptedOnly,
        AcknowledgedOnly
    }

    [SerializeField] private GateRule gateRule = GateRule.AcceptedOnly;

    [Header("Locked Behaviour")]
    [SerializeField] private DialogueSO lockedDialogue;     // 잠겨있을 때 재생할 대화
    [SerializeField] private string lockedLog = "Cannot enter."; // lockedDialogue가 없을 때 fallback 로그

    [Header("Minigame")]
    [SerializeField] private bool isMinigame = false;
    [SerializeField] private string returnSpawnPointId = "Default";
    [SerializeField] private string successSignalIdOnClear = "";

    /// <summary>
    /// 상호작용 진입점.
    /// - 퀘스트 조건을 만족하면 씬 이동
    /// - 조건 불만족 시 잠김 대화(lockedDialogue)를 재생
    /// </summary>
    public void Interact()
    {
        // 대화 UI가 이미 열려있으면 문 상호작용을 막는다.
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen)
            return;

        // 퀘스트 게이트가 켜져 있고 조건을 만족하지 못하면 잠김 처리
        if (useQuestGate && !CanEnterByQuest())
        {
            HandleLocked();
            return;
        }

        // 미니게임 진입이라면 복귀 정보를 저장한다.
        // - returnScene: 현재 씬
        // - returnSpawnPointId: 복귀 시 스폰 위치
        // - successSignalIdOnClear: 성공 시 발행할 퀘스트 시그널(비워두면 발행 안 함)
        if (isMinigame && MinigameFlow.Instance != null)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (System.Enum.TryParse(currentSceneName, out SceneName currentScene))
            {
                MinigameFlow.Instance.Begin(
                    returnScene: currentScene,
                    returnSpawnId: returnSpawnPointId,
                    successSignalId: successSignalIdOnClear
                );
            }
            else
            {
                Debug.LogWarning($"[DoorToScene] Cannot parse current scene '{currentSceneName}' to SceneName. Minigame return will not work.");
            }
        }

        // 다음 씬에서 스폰할 위치를 예약
        var spawner = FindFirstObjectByType<PlayerSpawnSystem>();
        if (spawner != null)
            spawner.SetNextSpawn(targetSpawnPointId);

        // 씬 전환(전환 연출 포함)
        TransitionManager.Instance.TransitionTo(targetScene, targetSpawnPointId);
    }

    /// <summary>
    /// requiredQuestId의 현재 상태를 확인하여 진입 가능 여부를 반환한다.
    /// - AcceptedOnly: Accepted일 때만 허용 (미니게임/제한 구역 진입 등)
    /// - AcknowledgedOnly: Acknowledged일 때만 허용 (완료 처리까지 끝난 맵 진입 등)
    /// </summary>
    private bool CanEnterByQuest()
    {
        // 퀘스트 ID를 비워두면 게이트 조건을 적용하지 않는다.
        if (string.IsNullOrEmpty(requiredQuestId))
            return true;

        // QuestManager가 없으면 안전하게 진입 불가 처리한다.
        if (QuestManager.Instance == null)
            return false;

        QuestState state = QuestManager.Instance.GetState(requiredQuestId);

        switch (gateRule)
        {
            case GateRule.AcceptedOnly:
                return state == QuestState.Accepted;

            case GateRule.AcknowledgedOnly:
                return state == QuestState.Acknowledged;

            default:
                return true;
        }
    }

    /// <summary>
    /// 잠김 상태 처리.
    /// - lockedDialogue가 있으면 대화를 시작한다.
    /// - 없으면 로그로만 안내한다.
    /// </summary>
    private void HandleLocked()
    {
        if (lockedDialogue != null && DialogueManager.Instance != null)
        {
            PlayDialogue(lockedDialogue);
            return;
        }

        Debug.Log($"[DoorToScene] Locked: {lockedLog}");
    }

    /// <summary>
    /// 잠김 안내 대화를 실행한다.
    /// DialogueManager의 StartDialogue(DialogueSO) API를 사용한다.
    /// </summary>
    private void PlayDialogue(DialogueSO dialogue)
    {
        DialogueManager.Instance.StartDialogue(dialogue);
    }
}