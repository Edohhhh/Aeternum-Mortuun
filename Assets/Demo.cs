using UnityEngine;
using EasyUI.PickerWheelUI;
using UnityEngine.UI;
using TMPro;

public class Demo : MonoBehaviour
{
    [SerializeField] private Button uiSpinButton;
    [SerializeField] private TextMeshProUGUI uiSpinButtonText;
    [SerializeField] private PickerWheel pickerWheel;

    [Header("UI Confirmación y Usos")]
    [SerializeField] private Button confirmarBoton;
    [SerializeField] private TextMeshProUGUI usosTexto;
    [SerializeField] private int usosDisponibles = 3;

    private WheelPiece premioPendiente;

    private void Start()
    {
        uiSpinButton.onClick.AddListener(SpinWheel);
        confirmarBoton.onClick.AddListener(ConfirmarPremio);

        pickerWheel.OnSpinEnd += OnSpinEnd;

        confirmarBoton.gameObject.SetActive(false);
        ActualizarUsosTexto();
    }

    private void SpinWheel()
    {
        if (usosDisponibles <= 0 || pickerWheel.IsSpinning)
            return;

        // Descartar premio anterior si no se confirmó
        if (premioPendiente != null)
        {
            Debug.Log("Premio descartado: " + premioPendiente.Label);
        }

        usosDisponibles--; // ✅ RESTAR USO AQUÍ
        ActualizarUsosTexto();

        premioPendiente = null;
        confirmarBoton.gameObject.SetActive(false);

        uiSpinButton.interactable = false;
        uiSpinButtonText.text = "Girando...";

        pickerWheel.Spin();
    }



    private void OnSpinEnd(WheelPiece wheelPiece)
    {
        Debug.Log("Pieza seleccionada: " + wheelPiece.Label);

        premioPendiente = wheelPiece;
        confirmarBoton.gameObject.SetActive(true);

        uiSpinButtonText.text = "Girar";
        uiSpinButton.interactable = usosDisponibles > 0; // Puedes girar de nuevo si aún tienes usos
    }


    private void ConfirmarPremio()
    {
        if (premioPendiente == null)
            return;

        Debug.Log("Premio confirmado: " + premioPendiente.Label + " x" + premioPendiente.Amount);

        AplicarPremio(premioPendiente);

        confirmarBoton.gameObject.SetActive(false);
        premioPendiente = null;

        uiSpinButton.interactable = usosDisponibles > 0;
        uiSpinButtonText.text = usosDisponibles > 0 ? "Girar" : "Sin Usos";
    }

    private void ActualizarUsosTexto()
    {
        usosTexto.text = "Usos: " + usosDisponibles;
    }

    private void AplicarPremio(WheelPiece pieza)
    {
        switch (pieza.Index)
        {
            case 0: Debug.Log("Salud aumentada en 20 puntos"); break;
            case 1: Debug.Log("Daño aumentado en 3 puntos"); break;
            case 2: Debug.Log("Cuchillo activado."); break;
            case 3: Debug.Log("Torreta activada."); break;
            case 4: Debug.Log("Bala extra añadida."); break;
            case 5: Debug.Log("Lanzallamas activado."); break;
            case 6: Debug.Log("Escudo activado."); break;
            case 7: Debug.Log("Mina activada."); break;
            default: Debug.Log("Premio sin efecto."); break;
        }
    }
}
