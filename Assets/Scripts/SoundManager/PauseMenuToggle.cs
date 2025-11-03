using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class PauseMenuToggle : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Panel que contiene las opciones/pause menu (GameObject UI)")]
    [SerializeField] private GameObject optionsPanel;

    [Tooltip("Primer selectable del panel para darle foco (opcional)")]
    [SerializeField] private GameObject firstSelectable;

    [Header("Comportamiento")]
    [Tooltip("¿Pausar el juego ajustando Time.timeScale?")]
    [SerializeField] private bool pauseTime = true;

    private bool _isOpen;

    private void Start()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        _isOpen = false;

        // Cursor siempre visible y libre
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleOptions();
        }
    }

    public void ToggleOptions()
    {
        if (optionsPanel == null) return;

        _isOpen = !_isOpen;
        optionsPanel.SetActive(_isOpen);

        // Pausar / reanudar el tiempo del juego
        if (pauseTime)
            Time.timeScale = _isOpen ? 0f : 1f;

        // 🔸 Cursor siempre visible y libre (sin importar el estado)
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Focus al primer elemento selectable para navegación con teclado/joystick
        if (_isOpen && firstSelectable != null)
        {
            EventSystem.current?.SetSelectedGameObject(null);
            EventSystem.current?.SetSelectedGameObject(firstSelectable);
        }
        else
        {
            EventSystem.current?.SetSelectedGameObject(null);
        }

        // Si el panel tiene AudioSettingsUI, forzamos a actualizar sus sliders
        var settings = optionsPanel.GetComponentInChildren<AudioSettingsUI>();
        if (settings != null)
        {
            var method = settings.GetType().GetMethod("RefreshUI");
            if (method != null)
                method.Invoke(settings, null);
        }
    }

    // Método público para cerrar desde botones UI (por ejemplo: Close button)
    public void CloseOptions()
    {
        if (_isOpen) ToggleOptions();
    }
}
