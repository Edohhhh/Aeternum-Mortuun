using UnityEngine;
using UnityEngine.EventSystems;
using EasyUI.PickerWheelUI;

public class RuletaClickable : MonoBehaviour, IPointerClickHandler
{
    private WheelSelector _selector;
    private PickerWheel _wheel;

    // ✅ ESTE es el ÚNICO método Setup
    public void Setup(WheelSelector selector, PickerWheel wheel)
    {
        _selector = selector;
        _wheel = wheel;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _selector.SeleccionarRuleta(_wheel);
    }
}
