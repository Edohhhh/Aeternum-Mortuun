using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string parameterName = "MusicVol"; // nombre exacto del parámetro expuesto

    [Header("Slider UI")]
    [SerializeField] private Slider volumeSlider;

    private void Start()
    {
        if (PlayerPrefs.HasKey(parameterName))
        {
            float savedVolume = PlayerPrefs.GetFloat(parameterName);
            volumeSlider.value = savedVolume;
            SetVolume(savedVolume);
        }

        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float sliderValue)
    {
        // Convertimos el valor (0–1) a decibelios (-80 a 0)
        float dB = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(parameterName, dB);

        // Guardamos la preferencia
        PlayerPrefs.SetFloat(parameterName, sliderValue);
    }
}
