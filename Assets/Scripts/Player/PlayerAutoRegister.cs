using UnityEngine;

public class PlayerAutoRegister : MonoBehaviour
{
    private void OnEnable()
    {
        PlayerRegistry.Instance?.Register(gameObject);
    }

    private void OnDisable()
    {
        PlayerRegistry.Instance?.Unregister(gameObject);
    }
}