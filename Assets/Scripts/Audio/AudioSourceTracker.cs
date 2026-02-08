using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceTracker : MonoBehaviour
{
    [SerializeField] private bool is3D = true;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 20f;

    private AudioSource src;

    private void Awake()
    {
        src = GetComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;

        src.spatialBlend = is3D ? 1f : 0f;
        if (is3D)
        {
            src.rolloffMode = AudioRolloffMode.Linear;
            src.minDistance = minDistance;
            src.maxDistance = maxDistance;
        }
    }

    public void PlayOneShot(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (!clip) return;
        src.pitch = pitch;
        src.volume = volume;
        src.PlayOneShot(clip);
    }

    private void OnDisable()
    {
        if (src != null) src.Stop();
    }
}