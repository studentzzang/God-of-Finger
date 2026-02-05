using System.Collections.Generic;
using UnityEngine;

public class RuntimeOnceFlags : MonoBehaviour
{
    public static RuntimeOnceFlags Instance { get; private set; }
    private readonly HashSet<string> shown = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool TryMarkShown(string key)
    {
        if (string.IsNullOrEmpty(key)) return true; // 키 없으면 항상 실행
        return shown.Add(key); // 처음이면 true, 이미 있으면 false
    }

    public void ClearAll() => shown.Clear();
}