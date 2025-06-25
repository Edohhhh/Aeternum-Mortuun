using UnityEngine;
using UnityEngine.UI;
using EasyUI.PickerWheelUI;

public class RuletaUISet : MonoBehaviour
{
    public Button selectButton;
    public Button spinButton;
    public Button confirmButton;

    [HideInInspector] public PickerWheel linkedWheel;

    public void Inicializar(PickerWheel wheel, WheelSelector selector)
    {
        linkedWheel = wheel;

        selectButton.onClick.AddListener(() => selector.SeleccionarRuletaDesdeBoton(this));
        spinButton.onClick.AddListener(() => selector.SpinRuleta(linkedWheel));
        confirmButton.onClick.AddListener(() => selector.ConfirmarRuleta(linkedWheel));

        // Desactivar botones hasta que se seleccione
        spinButton.interactable = false;
        confirmButton.interactable = false;
    }

    public void Activar(bool estado)
    {
        spinButton.interactable = estado;
        confirmButton.interactable = estado;
    }
}

