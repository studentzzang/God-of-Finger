using UnityEngine;

public class CameraRouter : MonoBehaviour
{
    [Header("Bootstrap World Camera")]
    [SerializeField] private Camera worldCamera;

    [Header("Optional")]
    [SerializeField] private bool manageAudioListener = true;

    public void UseWorld()
    {
        SetWorldCameraEnabled(true);
    }

    public void UseMinigame()
    {
        SetWorldCameraEnabled(false);
    }

    private void SetWorldCameraEnabled(bool enabled)
    {
        if (worldCamera != null)
            worldCamera.enabled = enabled;

        if (!manageAudioListener) return;

        if (worldCamera != null)
        {
            var listener = worldCamera.GetComponent<AudioListener>();
            if (listener != null)
                listener.enabled = enabled;
        }
    }
}