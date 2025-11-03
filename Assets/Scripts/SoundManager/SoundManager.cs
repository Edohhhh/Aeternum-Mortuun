using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[DisallowMultipleComponent]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Mixer (assign one)")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Mixer groups (assign the groups you will use here)")]
    [SerializeField] private AudioMixerGroup[] mixerGroups;

    [Header("Pool settings")]
    [Tooltip("Number of AudioSources to keep in the pool for overlapping SFX")]
    [SerializeField] private int poolSize = 16;

    [Header("Defaults")]
    [Tooltip("dB value to use when slider is 0 (silencio).")]
    [SerializeField] private float minDb = -80f;

    private Dictionary<string, AudioMixerGroup> _groupMap;
    private List<AudioSource> _pool;
    private Queue<AudioSource> _available;

    private void Awake()
    {
        // singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildGroupMap();
        BuildPool();
    }

    private void BuildGroupMap()
    {
        _groupMap = new Dictionary<string, AudioMixerGroup>();
        if (mixerGroups == null) return;
        foreach (var g in mixerGroups)
        {
            if (g == null) continue;
            if (!_groupMap.ContainsKey(g.name)) _groupMap.Add(g.name, g);
        }
    }

    private void BuildPool()
    {
        _pool = new List<AudioSource>(poolSize);
        _available = new Queue<AudioSource>(poolSize);

        for (int i = 0; i < poolSize; i++)
        {
            var srcGO = new GameObject($"SFX_Source_{i}");
            srcGO.transform.SetParent(transform, true);
            var src = srcGO.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f; // 2D by default
            // By default no clip assigned, use PlayOneShot/Play with assigned clip
            _pool.Add(src);
            _available.Enqueue(src);
        }
    }

    /// <summary>
    /// Play an AudioClip routing through the assigned AudioMixerGroup.
    /// Uses an internal pool for overlapping playback.
    /// </summary>
    public void PlayClip(AudioClip clip, string mixerGroupName = null, float volume = 1f)
    {
        if (clip == null) return;

        AudioMixerGroup group = null;
        if (!string.IsNullOrEmpty(mixerGroupName) && _groupMap != null)
        {
            _groupMap.TryGetValue(mixerGroupName, out group);
        }

        AudioSource src = GetPooledSource();
        if (src == null)
        {
            Debug.LogWarning("SoundManager: no pooled AudioSource available. Consider increasing poolSize.");
            return;
        }

        src.outputAudioMixerGroup = group;
        src.volume = Mathf.Clamp01(volume);
        // use PlayOneShot to avoid interrupting other playing clips on this source
        src.PlayOneShot(clip, 1f);

        // release the source after clip length (unscaled so timescale doesn't block)
        StartCoroutine(ReleaseAfterRealtime(src, clip.length));
    }

    /// <summary>
    /// Play with direct AudioMixerGroup reference.
    /// </summary>
    public void PlayClip(AudioClip clip, AudioMixerGroup group, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource src = GetPooledSource();
        if (src == null)
        {
            Debug.LogWarning("SoundManager: no pooled AudioSource available. Consider increasing poolSize.");
            return;
        }
        src.outputAudioMixerGroup = group;
        src.volume = Mathf.Clamp01(volume);
        src.PlayOneShot(clip, 1f);
        StartCoroutine(ReleaseAfterRealtime(src, clip.length));
    }

    private AudioSource GetPooledSource()
    {
        if (_available.Count > 0)
        {
            return _available.Dequeue();
        }

        // fallback: search for a source that is not playing
        foreach (var s in _pool)
        {
            if (!s.isPlaying) return s;
        }

        // no free source
        return null;
    }

    private IEnumerator ReleaseAfterRealtime(AudioSource src, float clipLength)
    {
        // clipLength might be 0 for dynamically generated clips; clamp
        float wait = Mathf.Max(0.05f, clipLength + 0.05f);
        // use realtime so timescale = 0 doesn't block returning the source
        float t = 0f;
        while (t < wait)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // ensure the source is stopped (PlayOneShot doesn't set clip, so just ensure not playing)
        // No need to set src.outputAudioMixerGroup = null (we keep it so subsequent plays reuse it)
        if (!_available.Contains(src))
            _available.Enqueue(src);
    }

    /// <summary>
    /// Set exposed parameter on the mixer (slider 0..1 converted to dB).
    /// </summary>
    public void SetMixerVolume(string exposedParameter, float linear01)
    {
        if (audioMixer == null || string.IsNullOrEmpty(exposedParameter)) return;

        float db;
        linear01 = Mathf.Clamp01(linear01);
        if (linear01 <= 0f) db = minDb;
        else db = Mathf.Log10(Mathf.Clamp(linear01, 0.0001f, 1f)) * 20f;

        audioMixer.SetFloat(exposedParameter, db);
    }

    public float GetMixerVolume(string exposedParameter)
    {
        if (audioMixer == null || string.IsNullOrEmpty(exposedParameter)) return 1f;
        if (audioMixer.GetFloat(exposedParameter, out float db))
        {
            if (db <= minDb) return 0f;
            return Mathf.Pow(10f, db / 20f);
        }
        return 1f;
    }

    /// <summary>
    /// Force-stop all pool audio immediately.
    /// </summary>
    public void StopAllSFX()
    {
        foreach (var s in _pool)
        {
            s.Stop();
        }
        // reset available queue
        _available.Clear();
        foreach (var s in _pool) _available.Enqueue(s);
    }
}
