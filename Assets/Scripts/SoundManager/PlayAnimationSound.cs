using UnityEngine;
using UnityEngine.Audio;

public class PlayAnimationSound : StateMachineBehaviour
{
    [Header("Configuración del sonido")]
    [Tooltip("Clip que se reproducirá al entrar en el estado.")]
    public AudioClip clip;

    [Tooltip("Grupo del AudioMixer al que pertenece este sonido.")]
    public AudioMixerGroup mixerGroup;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Tooltip("Si es true, el sonido se reproducirá en 3D según la posición del Animator.")]
    public bool spatial3D = false;

    // Se llama cuando se entra en el estado
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (clip == null) return;

        // Usa el SoundManager si está disponible
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayClip(clip, mixerGroup, volume);
            return;
        }

        // Fallback: crear un AudioSource local si no hay SoundManager
        var go = animator.gameObject;
        var src = go.GetComponent<AudioSource>();
        if (src == null) src = go.AddComponent<AudioSource>();

        src.playOnAwake = false;
        src.clip = clip;
        src.volume = Mathf.Clamp01(volume);
        src.outputAudioMixerGroup = mixerGroup;
        src.spatialBlend = spatial3D ? 1f : 0f;
        src.Play();
    }
}
