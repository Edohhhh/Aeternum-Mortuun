using UnityEngine;

namespace EasyUI.PickerWheelUI
{
    public class WheelUIController : MonoBehaviour
    {
        [Header("Canvas de la ruleta")]
        [SerializeField] private GameObject wheelCanvas;

        [Tooltip("¿La ruleta debe estar activa al iniciar el juego?")]
        [SerializeField] private bool startActive = false;

        [Header("Referencia al selector de ruletas")]
        [SerializeField] private WheelSelector wheelSelector;

        private void Start()
        {
            if (wheelCanvas != null)
            {
                wheelCanvas.SetActive(startActive);
            }

            if (startActive)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        public void MostrarRuleta()
        {
            if (wheelCanvas != null)
                wheelCanvas.SetActive(true);


            Time.timeScale = 0f;
        }

        public void ConfirmarPremio()
        {
            if (wheelCanvas != null)
                wheelCanvas.SetActive(false);

            Time.timeScale = 1f;
        }

        public void VerificarYMostrarSiNoHayEnemigos()
        {
            GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemy");

            if (enemigos.Length == 0 && !wheelCanvas.activeSelf)
            {
                MostrarRuleta();
            }
        }
    }
}
