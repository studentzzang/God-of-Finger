using UnityEngine;

public class NpcQuestVisibility : MonoBehaviour
{
    [Header("Show From (optional)")]
    [SerializeField] private string showQuestId;
    [SerializeField] private QuestState showWhen = QuestState.Completed;
    [SerializeField] private bool showAtLeast = true;

    [Header("Hide After (optional)")]
    [SerializeField] private string hideQuestId;
    [SerializeField] private QuestState hideWhen = QuestState.Completed;
    [SerializeField] private bool hideAtLeast = true;

    private bool pendingApply;

    private void OnEnable()
    {
        ApplyOrDefer();

        if (QuestManager.Instance != null)
            QuestManager.Instance.Revision.AddListener(OnRevisionChanged);

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnDialogueClosed += OnDialogueClosed;
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.Revision.RemoveListener(OnRevisionChanged);

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnDialogueClosed -= OnDialogueClosed;
    }

    private void OnRevisionChanged(int _)
    {
        ApplyOrDefer();
    }

    private void ApplyOrDefer()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen)
        {
            pendingApply = true;
            return;
        }

        pendingApply = false;
        Apply();
    }

    private void OnDialogueClosed()
    {
        if (!pendingApply) return;
        ApplyOrDefer();
    }

    private void Apply()
    {
        if (QuestManager.Instance == null)
        {
            gameObject.SetActive(false);
            return;
        }

        bool shouldShow = string.IsNullOrEmpty(showQuestId)
            ? true
            : Check(showQuestId, showWhen, showAtLeast);

        bool shouldHide = string.IsNullOrEmpty(hideQuestId)
            ? false
            : Check(hideQuestId, hideWhen, hideAtLeast);

        gameObject.SetActive(shouldShow && !shouldHide);
    }

    private bool Check(string questId, QuestState state, bool atLeast)
    {
        var current = QuestManager.Instance.GetState(questId);
        return atLeast ? current >= state : current == state;
    }
}