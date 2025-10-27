using UnityEngine;
using System;

public enum SoundType
{
    Atack,
    Hurt,
    Rodar,
    Walk,
}

[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundGroup[] soundGroups;
    private AudioSource audioSource;
    private static SoundManager instance;

    private void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(SoundType sound, float volume = 1)
    {
        SoundGroup group = instance.soundGroups[(int)sound];
        if (group.sounds == null || group.sounds.Length == 0)
        {
            Debug.LogWarning($"No clips asignados para {sound}");
            return;
        }

        AudioClip randomClip = group.sounds[UnityEngine.Random.Range(0, group.sounds.Length)];
        instance.audioSource.PlayOneShot(randomClip, volume);
    }
    public static void PlayLoop(SoundType sound, float volume = 1)
    {
        SoundGroup group = instance.soundGroups[(int)sound];
        if (group.sounds == null || group.sounds.Length == 0)
        {
            Debug.LogWarning($"No clips asignados para {sound}");
            return;
        }

        AudioClip randomClip = group.sounds[UnityEngine.Random.Range(0, group.sounds.Length)];
        instance.audioSource.clip = randomClip;
        instance.audioSource.volume = volume;
        instance.audioSource.loop = true;
        instance.audioSource.Play();
    }

    public static void Stop()
    {
        instance.audioSource.Stop();
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundGroups, names.Length);
        for (int i = 0; i < soundGroups.Length; i++)
        {
            soundGroups[i].name = names[i];
        }
    }
#endif
}

[Serializable]
public struct SoundGroup
{
    public string name;
    public AudioClip[] sounds;
}
