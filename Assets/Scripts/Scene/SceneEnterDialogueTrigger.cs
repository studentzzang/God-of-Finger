using System;

// If QuestState and QuestManager are in a namespace, add its using directive:
// using YourQuestNamespace;
using System.Collections;
using UnityEngine;

public class SceneEnterDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private DialogueSO dialogue;

    [Header("Run Once (runtime only)")]
    [SerializeField] private bool runOnce = true;
    [SerializeField] private string onceKey = ""; // 예: "HouseIntro"

    [Header("Timing")]
    [SerializeField] private int delayFrames = 1;

    [Header("Quest Condition (optional)")]
    [SerializeField] private bool useQuestGate = false;
    [SerializeField] private string requiredQuestId = "";
    [SerializeField] private QuestState requiredState = QuestState.Completed;
    [SerializeField] private bool requiredAtLeast = true;

    private void Start()
    {
        StartCoroutine(CoTryPlay());
    }

    private IEnumerator CoTryPlay()
    {
        for (int i = 0; i < delayFrames; i++)
            yield return null;

        if (dialogue == null) yield break;
        if (DialogueManager.Instance == null) yield break;

        while (DialogueManager.Instance.IsOpen)
            yield return null;

        if (useQuestGate)
        {
            if (QuestManager.Instance == null) yield break;
            if (string.IsNullOrEmpty(requiredQuestId)) yield break;

            var cur = QuestManager.Instance.GetState(requiredQuestId);
            bool ok = requiredAtLeast ? (cur >= requiredState) : (cur == requiredState);
            if (!ok) yield break;
        }

        if (runOnce)
        {
            if (RuntimeOnceFlags.Instance == null) yield break;

            string key = BuildKey();
            if (!RuntimeOnceFlags.Instance.TryMarkShown(key)) yield break;
        }

        DialogueManager.Instance.StartDialogue(dialogue);
    }

    private string BuildKey()
    {
        if (!string.IsNullOrEmpty(onceKey)) return onceKey;
        return $"{gameObject.scene.name}:{name}";
    }
}