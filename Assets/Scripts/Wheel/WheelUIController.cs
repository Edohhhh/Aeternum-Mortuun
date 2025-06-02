using UnityEngine;

namespace EasyUI.PickerWheelUI
{
    public class WheelUIController : MonoBehaviour
    {
        [SerializeField] private GameObject wheelCanvas;
        [Tooltip("¿La ruleta debe estar activa al iniciar el juego?")]
        [SerializeField] private bool startActive = false;

        private void Start()
        {
            if (wheelCanvas != null)
            {
                // Activa o desactiva según el bool del Inspector
                wheelCanvas.SetActive(startActive);
            }

            // Ajusta el timeScale según si arrancamos con ruleta activa o no
            Time.timeScale = startActive ? 0f : 1f;
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