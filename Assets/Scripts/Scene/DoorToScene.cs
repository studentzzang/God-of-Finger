using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorToScene : MonoBehaviour
{
    [SerializeField] private SceneName targetScene;
    [SerializeField] private string targetSpawnPointId = "Default";
    

    public void Interact()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen)
            return;
        // 다음 씬에서 어디로 스폰할지 예약
        var spawner = FindFirstObjectByType<PlayerSpawnSystem>();
        if (spawner != null)
            spawner.SetNextSpawn(targetSpawnPointId);

        TransitionManager.Instance.TransitionTo(targetScene, targetSpawnPointId);
        //SceneLoader.Instance.LoadScene(targetScene);
        
    }
}
