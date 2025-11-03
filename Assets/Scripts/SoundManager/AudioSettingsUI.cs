using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class AudioSettingsUI : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string musicParameter = "MusicVol"; // expuesto en el AudioMixer
    [SerializeField] private string sfxParameter = "SFXVol";     // expuesto en el AudioMixer

    [Header("UI Sliders (assign)")]
    [SerializeField] private Slider menuMusicSlider;
    [SerializeField] private Slider menuSfxSlider;
    [SerializeField] private Slider pauseMusicSlider;
    [SerializeField] private Slider pauseSfxSlider;

    [Header("Prefs keys")]
    [SerializeField] private string musicPrefKey = "MASTER_MUSIC";
    [SerializeField] private string sfxPrefKey = "MASTER_SFX";

    private void Start()
    {
        // Load saved values or read from mixer
        float musicVal = PlayerPrefs.HasKey(musicPrefKey) ? PlayerPrefs.GetFloat(musicPrefKey) : GetMixerValue(musicParameter);
        float sfxVal = PlayerPrefs.HasKey(sfxPrefKey) ? PlayerPrefs.GetFloat(sfxPrefKey) : GetMixerValue(sfxParameter);

        // assign to sliders and add listeners
        SetSliderIfExists(menuMusicSlider, musicVal, OnMusicSliderChanged);
        SetSliderIfExists(pauseMusicSlider, musicVal, OnMusicSliderChanged);

        SetSliderIfExists(menuSfxSlider, sfxVal, OnSfxSliderChanged);
        SetSliderIfExists(pauseSfxSlider, sfxVal, OnSfxSliderChanged);

        // apply immediately
        SetMusicVolume(musicVal);
        SetSfxVolume(sfxVal);
    }

    private float GetMixerValue(string param)
    {
        if (audioMixer == null) return 1f;
        if (audioMixer.GetFloat(param, out float db))
        {
            if (db <= -80f) return 0f;
            return Mathf.Pow(10f, db / 20f);
        }
        return 1f;
    }

    private void SetSliderIfExists(Slider s, float value, UnityEngine.Events.UnityAction<float> callback)
    {
        if (s == null) return;
        s.SetValueWithoutNotify(value);
        s.onValueChanged.AddListener(callback);
    }

    public void OnMusicSliderChanged(float v)
    {
        SetMusicVolume(v);
        PlayerPrefs.SetFloat(musicPrefKey, v);
    }

    public void OnSfxSliderChanged(float v)
    {
        SetSfxVolume(v);
        PlayerPrefs.SetFloat(sfxPrefKey, v);
    }

    public void SetMusicVolume(float linear01)
    {
        if (audioMixer == null)
        {
            Debug.LogWarning("AudioSettingsUI: AudioMixer not assigned.");
            return;
        }
        float db = (linear01 <= 0f) ? -80f : Mathf.Log10(Mathf.Clamp(linear01, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(musicParameter, db);
    }

    public void SetSfxVolume(float linear01)
    {
        if (audioMixer == null)
        {
            Debug.LogWarning("AudioSettingsUI: AudioMixer not assigned.");
            return;
        }
        float db = (linear01 <= 0f) ? -80f : Mathf.Log10(Mathf.Clamp(linear01, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(sfxParameter, db);
    }

    // ---- Nuevo: sincroniza sliders con valores actuales del AudioMixer
    public void RefreshUI()
    {
        float musicVal = GetMixerValue(musicParameter);
        float sfxVal = GetMixerValue(sfxParameter);

        if (menuMusicSlider != null) menuMusicSlider.SetValueWithoutNotify(musicVal);
        if (pauseMusicSlider != null) pauseMusicSlider.SetValueWithoutNotify(musicVal);
        if (menuSfxSlider != null) menuSfxSlider.SetValueWithoutNotify(sfxVal);
        if (pauseSfxSlider != null) pauseSfxSlider.SetValueWithoutNotify(sfxVal);
    }
}
