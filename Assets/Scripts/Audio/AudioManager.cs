using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Library")]
    [SerializeField] private AudioLibrary library;

    [Header("Volumes")]
    [Range(0f, 1f)] [SerializeField] private float master = 1f;
    [Range(0f, 1f)] [SerializeField] private float bgm = 1f;
    [Range(0f, 1f)] [SerializeField] private float sfx = 1f;
    [Range(0f, 1f)] [SerializeField] private float ui  = 1f;

    [Header("BGM Fade")]
    [SerializeField] private float bgmFadeSeconds = 0.6f;

    private AudioSource bgmA, bgmB;
    private bool usingA = true;

    private AudioSource oneShotSfx;
    private AudioSource oneShotUi;

    private AudioState state = AudioState.World;

    // key별 간단 쿨다운
    private readonly Dictionary<string, float> nextPlayable = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmA = CreateChildSource("BGM_A", loop: true, spatialBlend: 0f);
        bgmB = CreateChildSource("BGM_B", loop: true, spatialBlend: 0f);
        oneShotSfx = CreateChildSource("SFX_OneShot", loop: false, spatialBlend: 0f);
        oneShotUi  = CreateChildSource("UI_OneShot",  loop: false, spatialBlend: 0f);

        bgmA.volume = 0f;
        bgmB.volume = 0f;
    }

    private AudioSource CreateChildSource(string name, bool loop, float spatialBlend)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = loop;
        src.spatialBlend = spatialBlend; // 0=2D
        return src;
    }

    // ===== Public API =====

    public void SetLibrary(AudioLibrary lib) => library = lib;

    public void SetState(AudioState s) => state = s;

    public void SetVolumes(float master01, float bgm01, float sfx01, float ui01)
    {
        master = Mathf.Clamp01(master01);
        bgm = Mathf.Clamp01(bgm01);
        sfx = Mathf.Clamp01(sfx01);
        ui = Mathf.Clamp01(ui01);
    }

    public void PlayBGM(string key)
    {
        if (!TryBuild(key, out var entry, out var clip, out float vol, out float pitch))
            return;

        if (entry.bus != AudioBus.BGM)
        {
            Debug.LogWarning($"[AudioManager] '{key}' bus is {entry.bus}, expected BGM");
        }

        var from = usingA ? bgmA : bgmB;
        var to   = usingA ? bgmB : bgmA;
        usingA = !usingA;

        to.clip = clip;
        to.pitch = 1f;       // BGM은 보통 pitch 고정
        to.volume = 0f;
        to.Play();

        StopAllCoroutines();
        StartCoroutine(CrossFade(from, to, bgmFadeSeconds, targetVol: FinalBusVolume(AudioBus.BGM)));
    }

    public void StopBGM(float fadeSeconds = 0.3f)
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut(bgmA, fadeSeconds));
        StartCoroutine(FadeOut(bgmB, fadeSeconds));
    }

    public void PlaySFX(string key, float volumeMul = 1f) => PlayOneShot(key, AudioBus.SFX, volumeMul);
    public void PlayUI(string key, float volumeMul = 1f)  => PlayOneShot(key, AudioBus.UI,  volumeMul);

    // ===== Internals =====

    private void PlayOneShot(string key, AudioBus expected, float volumeMul)
    {
        if (!TryBuild(key, out var entry, out var clip, out float vol, out float pitch))
            return;

        if (entry.bus != expected)
        {
            // 버스가 달라도 실행은 하되(빨리 프로토), 실수 감지용
            // Debug.LogWarning($"[AudioManager] '{key}' bus is {entry.bus}, expected {expected}");
        }

        if (!PassCooldown(entry, key)) return;

        var src = entry.bus == AudioBus.UI ? oneShotUi : oneShotSfx;
        src.pitch = pitch;
        src.volume = vol * volumeMul;
        src.PlayOneShot(clip);
    }

    private bool TryBuild(string key, out AudioClipEntry entry, out AudioClip clip, out float vol, out float pitch)
    {
        entry = null; clip = null; vol = 0f; pitch = 1f;

        if (library == null)
        {
            Debug.LogWarning("[AudioManager] library is null");
            return false;
        }

        if (!library.TryGet(key, out entry) || entry.clips == null || entry.clips.Length == 0)
        {
            Debug.LogWarning($"[AudioManager] key not found or empty: {key}");
            return false;
        }

        clip = entry.clips[Random.Range(0, entry.clips.Length)];
        if (clip == null) return false;

        pitch = (Mathf.Approximately(entry.pitchMin, entry.pitchMax))
            ? entry.pitchMin
            : Random.Range(entry.pitchMin, entry.pitchMax);

        vol = entry.volume * FinalBusVolume(entry.bus);
        return true;
    }

    private float FinalBusVolume(AudioBus bus)
    {
        float busVol = bus switch
        {
            AudioBus.BGM => bgm,
            AudioBus.UI => ui,
            _ => sfx
        };

        return master * busVol * StateMultiplier(bus);
    }

    private float StateMultiplier(AudioBus bus)
    {
        // 일단 가벼운 보정만. (원하면 여기서 미세 조정)
        return state switch
        {
            AudioState.Minigame  => bus == AudioBus.BGM ? 0.9f : 0.9f,
            AudioState.Cinematic => bus == AudioBus.BGM ? 0.8f : 0.8f,
            _ => 1f
        };
    }

    private bool PassCooldown(AudioClipEntry entry, string key)
    {
        if (entry.cooldown <= 0f) return true;

        float now = Time.unscaledTime;
        if (nextPlayable.TryGetValue(key, out float t) && now < t)
            return false;

        nextPlayable[key] = now + entry.cooldown;
        return true;
    }

    private IEnumerator CrossFade(AudioSource from, AudioSource to, float seconds, float targetVol)
    {
        float t = 0f;
        float fromStart = from.volume;

        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float a = seconds <= 0f ? 1f : Mathf.Clamp01(t / seconds);

            to.volume = Mathf.Lerp(0f, targetVol, a);
            from.volume = Mathf.Lerp(fromStart, 0f, a);
            yield return null;
        }

        to.volume = targetVol;
        from.volume = 0f;
        from.Stop();
    }

    private IEnumerator FadeOut(AudioSource src, float seconds)
    {
        float t = 0f;
        float start = src.volume;

        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float a = seconds <= 0f ? 1f : Mathf.Clamp01(t / seconds);
            src.volume = Mathf.Lerp(start, 0f, a);
            yield return null;
        }

        src.volume = 0f;
        src.Stop();
    }
}