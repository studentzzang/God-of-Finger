using System;
using System.Reflection;
using UnityEngine;
using TMPro;

public class TextByVariable : MonoBehaviour
{
    [Header("Target")]
    public MonoBehaviour targetScript;   // 값을 가져올스크립트
    public string variableName;           // 변수명

    [Header("UI")]
    public TMP_Text text;

    [Header("Option")]
    public string format = "{}";          //포맷할문자열
    public float refreshInterval = 0.1f;  // 갱신 주기

    FieldInfo _field;
    PropertyInfo _property;
    float _timer;

    void Awake()
    {
        if (targetScript == null || string.IsNullOrEmpty(variableName))
            return;

        Type type = targetScript.GetType();

        // 변수 우선 탐색
        _field = type.GetField(
            variableName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        );

        // field 없으면 property
        if (_field == null)
        {
            _property = type.GetProperty(
                variableName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );
        }

        /*
        if (_field == null && _property == null)
        {
            Debug.LogError("없음");
        }*/
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < refreshInterval) return;
        _timer = 0f;

        UpdateText();
    }

    void UpdateText()
    {
        if (text == null || targetScript == null) return;

        object value = null;

        if (_field != null)
            value = _field.GetValue(targetScript);
        else if (_property != null)
            value = _property.GetValue(targetScript);

        if (value == null)
            text.text = "null";
        else
            text.text = string.Format(format, value);
    }
}
