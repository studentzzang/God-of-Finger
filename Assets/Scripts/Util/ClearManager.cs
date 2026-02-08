using UnityEngine;
using System.Reflection;
using System.Collections;
public class ClearFinder : MonoBehaviour
{
    public Transform target;        // 다른 오브젝트 가능
    public string scriptName;       // 검사할 스크립트 이름

    MonoBehaviour targetScript;
    FieldInfo clearField;
    bool cleared;

    public float _delaySec = 1.9f;
    void Awake()
    {
        var scripts = target.GetComponents<MonoBehaviour>();
        for (int i = 0; i < scripts.Length; i++)
        {
            if (scripts[i].GetType().Name == scriptName)
            {
                targetScript = scripts[i];
                break;
            }
        }

        clearField = targetScript.GetType()
            .GetField("_clear", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }

    void Update()
    {
        if (cleared) return;

        if ((bool)clearField.GetValue(targetScript))
        {
            cleared = true;
            OnClear();
        }
    }

    void OnClear()
    {
        StartCoroutine(ExitAfterSeconds(_delaySec)); // n초
    }

    IEnumerator ExitAfterSeconds(float delay)
    {
        yield return new WaitForSeconds(delay);
        MinigameFlow.Instance.Exit(true);
    }
}
