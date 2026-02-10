using UnityEngine;
using System;

public class PlayerRegistry : MonoBehaviour
{
    public static PlayerRegistry Instance { get; private set; }

    public GameObject CurrentPlayer { get; private set; }
    public event Action<GameObject> OnPlayerChanged;

    [Header("Spawn")]
    [Tooltip("Transition 중 다음 플레이어가 등록되면 이 spawnId로 스폰 위치를 적용합니다.")]
    [SerializeField] private string pendingSpawnId;

    /// <summary>
    /// 다음에 등록될 플레이어에게 적용할 스폰 ID를 예약한다.
    /// (플레이어가 아직 생성되지 않은 타이밍에서도 안전)
    /// </summary>
    public void SetPendingSpawn(string spawnId)
    {
        pendingSpawnId = spawnId;
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Register(GameObject player)
    {
        CurrentPlayer = player;
        OnPlayerChanged?.Invoke(player);
        
        // 씬 로드 직후에는 Player가 아직 생성/활성화되지 않아 스폰 적용이 실패할 수 있음.
        // 따라서 TransitionManager에서 예약해둔 spawnId가 있으면, "등록되는 순간"에 스폰을 적용한다.
        if (!string.IsNullOrEmpty(pendingSpawnId))
        {
            ApplySpawn(player, pendingSpawnId);
            pendingSpawnId = null;
        }
    }

    private void ApplySpawn(GameObject player, string spawnId)
    {
        if (!player) return;

        var spawns = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        foreach (var s in spawns)
        {
            if (s != null && s.spawnId == spawnId)
            {
                player.transform.position = s.transform.position;
                return;
            }
        }
    }

    public void Unregister(GameObject player)
    {
        if (CurrentPlayer == player)
        {
            CurrentPlayer = null;
            OnPlayerChanged?.Invoke(null);
        }
    }
}