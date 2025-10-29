using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIHoverSound : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;   // Asign� el AudioSource del bot�n
    [SerializeField] private AudioClip hoverClip;       // Sonido al pasar el mouse
    [SerializeField] private AudioClip clickClip;       // (Opcional) Sonido al hacer click
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    private Button button;

    private void Reset()
    {
        button = GetComponent<Button>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(PlayClick);

        // Seguridad: si no hay AudioSource en el objeto, lo creamos
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayHover();
    }

    // Para navegaci�n con teclado/gamepad (cuando el bot�n se selecciona)
    public void ISelectHandler_OnSelect()
    {
        PlayHover();
    }
    void ISelectHandler.OnSelect(BaseEventData eventData) => ISelectHandler_OnSelect();

    private void PlayHover()
    {
        if (hoverClip == null || audioSource == null) return;

        // Evita acumular capas si te mov�s r�pido entre botones
        audioSource.Stop();
        audioSource.PlayOneShot(hoverClip, volume);
    }

    private void PlayClick()
    {
        if (clickClip == null || audioSource == null) return;
        audioSource.PlayOneShot(clickClip, volume);
    }
}
