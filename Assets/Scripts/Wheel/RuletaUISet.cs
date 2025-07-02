using UnityEngine;
using UnityEngine.UI;
using EasyUI.PickerWheelUI;

public class RuletaUISet : MonoBehaviour
{
    public Button selectButton;
    public Button spinButton;
    public Button confirmButton;

    private Text spinButtonText;

    [HideInInspector] public PickerWheel linkedWheel;

    public void Inicializar(PickerWheel wheel, WheelSelector selector)
    {
        linkedWheel = wheel;

        spinButtonText = spinButton.GetComponentInChildren<Text>();
        ActualizarTextoSpin();

        selectButton.onClick.AddListener(() => selector.SeleccionarRuletaDesdeBoton(this));
        spinButton.onClick.AddListener(() => selector.SpinRuleta(linkedWheel));
        confirmButton.onClick.AddListener(() => selector.ConfirmarRuleta(linkedWheel));

        // 🔁 Vincular actualización al final del spin
        linkedWheel.AddSpinEndListener((_) => ActualizarTextoSpin());

        spinButton.interactable = false;
        confirmButton.interactable = false;
    }

    public void Activar(bool estado)
    {
        spinButton.interactable = estado;
        confirmButton.interactable = estado;
    }

    public void ActualizarTextoSpin()
    {
        if (linkedWheel != null && spinButtonText != null)
        {
            spinButtonText.text = $"Spin ({linkedWheel.UsosRestantes}/3)";
        }
    }
}