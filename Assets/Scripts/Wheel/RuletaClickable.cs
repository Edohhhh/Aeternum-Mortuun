using UnityEngine;
using UnityEngine.EventSystems;
using EasyUI.PickerWheelUI;

public class RuletaClickable : MonoBehaviour, IPointerClickHandler
{
    private WheelSelector _selector;
    private PickerWheel _wheel;

    public void Setup(WheelSelector selector, PickerWheel wheel)
    {
        _selector = selector;
        _wheel = wheel;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // ❌ ESTA LÍNEA DABA ERROR PORQUE EL MÉTODO YA NO EXISTE.
        // _selector.SeleccionarRuleta(_wheel); 

        // ✅ AHORA: Solo mostramos un mensaje o no hacemos nada.
        // La interacción real se hace con los botones de la UI (RuletaUISet).
        Debug.Log("Has hecho click en la ruleta, pero debes usar el botón 'Girar' de la interfaz.");
    }
}