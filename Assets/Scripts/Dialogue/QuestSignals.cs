using System;
using UnityEngine;

/// <summary>
/// 게임 전역에서 퀘스트 관련 신호를 발행하는 간단한 이벤트 버스.
/// </summary>
public static class QuestSignals
{
    public static event Action<string> OnSignal;

    public static void Raise(string signalId)
    {
        if (string.IsNullOrEmpty(signalId)) return;
        Debug.Log($"[QuestSignal] {signalId}");
        OnSignal?.Invoke(signalId);
    }
    
}