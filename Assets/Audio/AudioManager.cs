using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
        public bool loop = false;
    }

    public Sound[] sounds;

    private Dictionary<string, AudioSource> soundMap = new Dictionary<string, AudioSource>();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var s in sounds)
        {
            if (s.clip == null || string.IsNullOrWhiteSpace(s.name))
            {
                Debug.LogWarning($"AudioManager: Skipping invalid sound entry.");
                continue;
            }

            GameObject go = new GameObject("Sound_" + s.name.ToLower());
            go.transform.SetParent(transform);

            AudioSource source = go.AddComponent<AudioSource>();
            source.clip = s.clip;
            source.volume = s.volume;
            source.pitch = s.pitch;
            source.loop = s.loop;
            source.playOnAwake = false;
            source.spatialBlend = 0f; // 2D sound

            soundMap[s.name.ToLower()] = source;
        }
    }

    public void Play(string name)
    {
        name = name.ToLower();
        if (soundMap.TryGetValue(name, out AudioSource source))
        {
            source.PlayOneShot(source.clip);
        }
        else
        {
            Debug.LogWarning($"[AudioManager] Sound not found: {name}");
        }
    }

    public void Stop(string name)
    {
        name = name.ToLower();
        if (soundMap.TryGetValue(name, out AudioSource source))
        {
            source.Stop();
        }
    }

    public void SetPitch(string name, float newPitch)
    {
        name = name.ToLower();
        if (soundMap.TryGetValue(name, out AudioSource source))
        {
            source.pitch = newPitch;
        }
    }

    public void SetVolume(string name, float newVolume)
    {
        name = name.ToLower();
        if (soundMap.TryGetValue(name, out AudioSource source))
        {
            source.volume = newVolume;
        }
    }

    public void PlayRandom(string[] names)
    {
        if (names.Length == 0) return;
        Play(names[Random.Range(0, names.Length)]);
    }

    public void PlayWithRandomPitch(string name, float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        name = name.ToLower();
        if (soundMap.TryGetValue(name, out AudioSource source))
        {
            float originalPitch = source.pitch;
            source.pitch = Random.Range(minPitch, maxPitch);
            source.PlayOneShot(source.clip);
            source.pitch = originalPitch;
        }
    }
}
