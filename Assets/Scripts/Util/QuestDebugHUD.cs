using System.Text;
using UnityEngine;

/// <summary>
/// 화면에 현재 퀘스트 상태를 표시하는 디버그 HUD.
/// </summary>
public class QuestDebugHUD : MonoBehaviour
{
    [SerializeField] private bool show = true;

    private void Update()
    {
        // F1로 HUD 토글
        if (Input.GetKeyDown(KeyCode.F1))
            show = !show;
    }

    private void OnGUI()
    {
        if (!show) return;

        var dict = QuestManager.Instance.GetAllStates();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("[Quest States]");
        foreach (var kv in dict)
            sb.AppendLine($"{kv.Key} : {kv.Value}");



        GUI.Label(new Rect(10, 10, 500, 800), sb.ToString());
    }
}