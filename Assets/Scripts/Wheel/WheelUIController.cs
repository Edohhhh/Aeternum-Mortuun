using UnityEngine;

namespace EasyUI.PickerWheelUI
{
    public class WheelUIController : MonoBehaviour
    {
        [SerializeField] private GameObject wheelCanvas;

        private void Start()
        {
            // Ocultar al inicio y asegurarse de que el juego no esté pausado
            if (wheelCanvas != null)
                wheelCanvas.SetActive(false);

            Time.timeScale = 1f; // Asegura que el juego comience sin pausa
        }

        public void ConfirmarPremio()
        {
            if (wheelCanvas != null)
                wheelCanvas.SetActive(false);

            Time.timeScale = 1f; // Reanudar el juego
        }

        public void MostrarRuleta()
        {
            if (wheelCanvas != null)
                wheelCanvas.SetActive(true);

            Time.timeScale = 0f; // Pausar el juego
        }

        public void VerificarYMostrarSiNoHayEnemigos()
        {
            GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemy");

            if (enemigos.Length == 0)
                MostrarRuleta();
        }
    }
}