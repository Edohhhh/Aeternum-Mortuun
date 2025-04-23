using UnityEngine;
using EasyUI.PickerWheelUI;
using UnityEngine.UI;
using TMPro;

public class Demo : MonoBehaviour
{
    [SerializeField] private Button uiSpinButton;
    [SerializeField] private TextMeshProUGUI uiSpinButtonText;
    [SerializeField] private PickerWheel pickerWheel;

    private void Start()
    {
        uiSpinButton.onClick.AddListener(() =>
        {
            uiSpinButton.interactable = false;
            uiSpinButtonText.text = "Spinning";

            pickerWheel.OnSpinStart(() =>
            {
                Debug.Log("Spin Started...");
            });

            pickerWheel.OnSpinEnd(wheelPiece =>
            {
                Debug.Log("Pieza seleccionada: " + wheelPiece.Label);

                switch (wheelPiece.Index)
                {
                    case 0:
                        Debug.Log("Resultado: Salud aumentada en 20 puntos");
                        break;

                    case 1:
                        Debug.Log("Resultado: Daño de bala aumentado en 3 puntos");
                        break;

                    case 2:
                        Debug.Log("Resultado: Cuchillo activado).");
                        break;

                    case 3:
                        Debug.Log("Resultado: Torreta activada.");
                        break;

                    case 4:
                        Debug.Log("Resultado: Bala extra añadida al pool.");
                        break;

                    case 5:
                        Debug.Log("Resultado: Lanzallamas activado.");
                        break;

                    case 6:
                        Debug.Log("Resultado: Escudo reflectante activado.");
                        break;

                    case 7:
                        Debug.Log("Resultado: Mina activada.");
                        break;

                    default:
                        Debug.Log("Resultado: No se asignó ningún efecto a esta pieza.");
                        break;
                }

                uiSpinButton.interactable = true;
                uiSpinButtonText.text = "Spin";
            });

            pickerWheel.Spin();
        });
    }
}

