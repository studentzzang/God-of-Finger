using UnityEngine;

public class QuitMinigame : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MinigameFlow.Instance.Exit(false);
        }
    }
}