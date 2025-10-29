using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SFXVolumeController : MonoBehaviour
{
    [Header("Slider de SFX (opcional)")]
    [SerializeField] private Slider slider;

    [Header("Prefs")]
    [SerializeField] private string prefsKey = "SFX_VOLUME"; // clave única
    [SerializeField, Range(0, 1)] private float defaultValue = 1f;

    private void Awake()
    {
        float v = PlayerPrefs.GetFloat(prefsKey, defaultValue);
        Apply(v);

        if (slider != null)
        {
            slider.SetValueWithoutNotify(v);
            slider.onValueChanged.AddListener(SetVolumeFromUI);
        }
    }

    public void SetVolumeFromUI(float v) => SetVolume(v);

    public void SetVolume(float v01)
    {
        v01 = Mathf.Clamp01(v01);
        PlayerPrefs.SetFloat(prefsKey, v01);
        Apply(v01);
    }

    public float GetVolume() => PlayerPrefs.GetFloat(prefsKey, defaultValue);

    private void Apply(float v01)
    {
        // Aplica el volumen sólo al SoundManager si existe
        var sm = FindObjectOfType<SoundManager>();
        if (sm != null)
        {
            var src = sm.GetComponent<AudioSource>();
            if (src != null)
                src.volume = v01;
        }
    }
}
