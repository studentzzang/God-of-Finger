using System;
using UnityEngine;

[Serializable]
public class AudioClipEntry
{
    public string key;                  // "BGM_Town", "SFX_DoorOpen", "UI_Click" 등
    public AudioBus bus = AudioBus.SFX;

    [Header("Clips")]
    public AudioClip[] clips;

    [Header("Defaults")]
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.5f, 2f)] public float pitchMin = 1f;
    [Range(0.5f, 2f)] public float pitchMax = 1f;

    [Tooltip("같은 키 연속 재생 제한(초). 0이면 제한 없음")]
    public float cooldown = 0f;
}

[CreateAssetMenu(menuName = "Audio/Audio Library", fileName = "AudioLibrary")]
public class AudioLibrary : ScriptableObject
{
    public AudioClipEntry[] entries;

    public bool TryGet(string key, out AudioClipEntry entry)
    {
        if (entries != null)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                var e = entries[i];
                if (e != null && e.key == key)
                {
                    entry = e;
                    return true;
                }
            }
        }
        entry = null;
        return false;
    }
}