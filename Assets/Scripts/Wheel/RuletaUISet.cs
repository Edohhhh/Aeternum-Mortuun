using UnityEngine;
using UnityEngine.UI;
using EasyUI.PickerWheelUI;
using TMPro;

public class RuletaUISet : MonoBehaviour
{
    [Header("Contenedor de botones")]
    public Transform buttonsContainer;

    [HideInInspector] public PickerWheel linkedWheel;

    public Button selectButton;
    public Button spinButton;
    public Button confirmButton;

    // Referencia al texto para poner (2/3)
    public TextMeshProUGUI spinButtonText;

    private CanvasGroup ruletaCanvasGroup;

    public void Inicializar(PickerWheel wheel, WheelSelector selector)
    {
        linkedWheel = wheel;

        ruletaCanvasGroup = linkedWheel.GetComponent<CanvasGroup>();
        if (ruletaCanvasGroup == null)
            ruletaCanvasGroup = linkedWheel.gameObject.AddComponent<CanvasGroup>();

        RuletaTheme theme = linkedWheel.GetComponent<RuletaTheme>();
        if (theme != null)
        {
            // 1. Select
            if (theme.selectButtonPrefab != null && selectButton == null)
            {
                var obj = Instantiate(theme.selectButtonPrefab, buttonsContainer);
                selectButton = obj.GetComponent<Button>();
                SetText(selectButton, "SELECCIONAR");
            }
            // 2. Spin
            if (theme.spinButtonPrefab != null && spinButton == null)
            {
                var obj = Instantiate(theme.spinButtonPrefab, buttonsContainer);
                spinButton = obj.GetComponent<Button>();
                spinButtonText = spinButton.GetComponentInChildren<TextMeshProUGUI>();
                SetText(spinButton, "GIRAR");
            }
            // 3. Confirm
            if (theme.confirmButtonPrefab != null && confirmButton == null)
            {
                var obj = Instantiate(theme.confirmButtonPrefab, buttonsContainer);
                confirmButton = obj.GetComponent<Button>();
                SetText(confirmButton, "CONFIRMAR");
            }
            // Fallback si no hay prefab de confirm
            else if (theme.spinButtonPrefab != null && confirmButton == null)
            {
                var obj = Instantiate(theme.spinButtonPrefab, buttonsContainer);
                confirmButton = obj.GetComponent<Button>();
                SetText(confirmButton, "CONFIRMAR");
            }
        }

        // Conectar funciones
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => selector.SeleccionarRuletaDesdeBoton(this));
        }
        if (spinButton != null)
        {
            spinButton.onClick.RemoveAllListeners();
            spinButton.onClick.AddListener(() => selector.SpinRuleta(linkedWheel));
        }
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() => selector.ConfirmarPremio());
        }

        ModoSeleccion();
    }

    private void SetText(Button btn, string text)
    {
        if (btn != null)
        {
            var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = text;
        }
    }

    // ✅ ACTUALIZAR TEXTO (Para el Perk)
    public void ActualizarTextoSpin(int actuales, int totales)
    {
        if (spinButtonText != null)
            spinButtonText.text = $"GIRAR ({actuales}/{totales})";
    }

    // --- ESTADOS VISUALES (Clásicos: Uno a la vez) ---

    public void ModoSeleccion()
    {
        ActivarCanvas(true);
        ToggleButtons(select: true);
    }

    public void ModoGiro()
    {
        ActivarCanvas(true);
        ToggleButtons(spin: true);
        if (spinButton != null) spinButton.interactable = true;
    }

    public void ModoGirando()
    {
        // Antes desactivabas el botón aquí.
        // Lo dejamos vacío para que el botón Spin siga activo visualmente.
        // El bloqueo real de múltiples giros lo hace wheel.IsSpinning en WheelSelector.
    }

    public void ModoConfirmar()
    {
        ActivarCanvas(true);
        ToggleButtons(confirm: true);
    }

    // ✅ NUEVO: mostrar Spin + Confirmar al mismo tiempo
    public void ModoSpinYConfirmar()
    {
        ActivarCanvas(true);
        ToggleButtons(spin: true, confirm: true);

        if (spinButton != null)
            spinButton.interactable = true;
    }

    public void Desactivar(bool completo)
    {
        ActivarCanvas(false); // Oscurecer
        ToggleButtons();      // Ocultar todo
    }

    private void ToggleButtons(bool select = false, bool spin = false, bool confirm = false)
    {
        if (selectButton != null) selectButton.gameObject.SetActive(select);
        if (spinButton != null) spinButton.gameObject.SetActive(spin);
        if (confirmButton != null) confirmButton.gameObject.SetActive(confirm);
    }

    private void ActivarCanvas(bool activo)
    {
        if (ruletaCanvasGroup != null)
        {
            ruletaCanvasGroup.alpha = activo ? 1f : 0.2f;
            ruletaCanvasGroup.interactable = activo;
            ruletaCanvasGroup.blocksRaycasts = activo;
        }
    }
}
