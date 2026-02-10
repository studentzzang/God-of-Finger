using System.Collections;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    [SerializeField] private SceneName targetScene = SceneName.House;
    [SerializeField] private string targetSpawnPointId = "Default";

    private IEnumerator Start()
    {
        // Bootstrap 내부 매니저들이 Awake/Start 준비할 한 박자
        yield return null;

        // 전환 파이프라인 단일 진입 (스폰 ID도 같이 넘김)
        TransitionManager.Instance.TransitionTo(targetScene, targetSpawnPointId);
    }
}