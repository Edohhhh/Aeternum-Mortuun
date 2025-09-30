using UnityEngine;
using UnityEngine.UI;
using EasyUI.PickerWheelUI;

public class RuletaUISet : MonoBehaviour
{
    [Header("Contenedor donde se instancian los botones")]
    public Transform buttonsContainer;

    [HideInInspector] public PickerWheel linkedWheel;

    public Button selectButton;
    public Button spinButton;
    public Button confirmButton;
    public Text spinButtonText;

    private CanvasGroup ruletaCanvasGroup;

    public void Inicializar(PickerWheel wheel, WheelSelector selector)
    {
        linkedWheel = wheel;

        // CanvasGroup para oscurecer la ruleta
        ruletaCanvasGroup = linkedWheel.GetComponent<CanvasGroup>();
        if (ruletaCanvasGroup == null)
            ruletaCanvasGroup = linkedWheel.gameObject.AddComponent<CanvasGroup>();

        // 🎨 Buscar el tema configurado en el prefab de la ruleta
        RuletaTheme theme = linkedWheel.GetComponent<RuletaTheme>();
        if (theme != null)
        {
            // Instanciar prefabs de botones
            if (theme.selectButtonPrefab != null)
            {
                var obj = Instantiate(theme.selectButtonPrefab, buttonsContainer);
                selectButton = obj.GetComponent<Button>();
            }

            if (theme.spinButtonPrefab != null)
            {
                var obj = Instantiate(theme.spinButtonPrefab, buttonsContainer);
                spinButton = obj.GetComponent<Button>();
                spinButtonText = spinButton.GetComponentInChildren<Text>();
            }

            if (theme.confirmButtonPrefab != null)
            {
                var obj = Instantiate(theme.confirmButtonPrefab, buttonsContainer);
                confirmButton = obj.GetComponent<Button>();
            }
        }

        // 🔗 Listeners de botones
        if (selectButton != null)
            selectButton.onClick.AddListener(() => selector.SeleccionarRuletaDesdeBoton(this));

        if (spinButton != null)
        {
            spinButton.onClick.AddListener(() => selector.SpinRuleta(linkedWheel));
            spinButton.interactable = false;
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(() => selector.ConfirmarRuleta(linkedWheel));
            confirmButton.interactable = false;
        }

        // 🔁 Actualizar texto de Spin después de girar
        if (linkedWheel != null)
            linkedWheel.AddSpinEndListener((_) => ActualizarTextoSpin());

        ActualizarTextoSpin();
    }

    public void Activar(bool estado)
    {
        if (spinButton != null) spinButton.interactable = estado;
        if (confirmButton != null) confirmButton.interactable = estado;

        if (ruletaCanvasGroup != null)
        {
            ruletaCanvasGroup.alpha = estado ? 1f : 0.2f;
            ruletaCanvasGroup.interactable = estado;
            ruletaCanvasGroup.blocksRaycasts = estado;
        }
    }

    public void ActualizarTextoSpin()
    {
        if (linkedWheel != null && spinButtonText != null)
        {
            spinButtonText.text = $"Spin ({linkedWheel.UsosRestantes}/3)";
        }
    }
}
