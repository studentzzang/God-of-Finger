using System;
using System.Reflection;
using UnityEngine;
using TMPro;

public class TextByVariable : MonoBehaviour
{
    [Header("Target")]
    public MonoBehaviour targetScript;
    public string variableName;

    [Header("UI")]
    public TMP_Text text;

    [Header("Option")]
    public string format = "{0}";
    public float refreshInterval = 0.1f;

    FieldInfo _field;
    PropertyInfo _property;
    float _timer;

    void Awake()
    {
        CacheMember();
        UpdateText(); // 시작하자마자 1회 갱신
    }

    void OnValidate()
    {
        // 인스펙터에서 값 바꿀 때 바로 반영
        CacheMember();
    }

    void CacheMember()
    {
        _field = null;
        _property = null;

        if (targetScript == null || string.IsNullOrEmpty(variableName))
            return;

        var type = targetScript.GetType();

        _field = type.GetField(variableName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (_field == null)
        {
            _property = type.GetProperty(variableName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // 인덱서 프로퍼티(예: this[int])면 GetValue에 인자 필요해서 제외
            if (_property != null && _property.GetIndexParameters().Length > 0)
                _property = null;
        }
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

        object value;

        try
        {
            if (_field != null) value = _field.GetValue(targetScript);
            else if (_property != null) value = _property.GetValue(targetScript);
            else { text.text = "null"; return; }
        }
        catch (Exception e)
        {
            text.text = $"err: {e.GetType().Name}";
            return;
        }

        // 핵심: format은 "{0}" 기반이어야 함
        if (value == null) text.text = "null";
        else text.text = string.Format(format, value);
    }
}
