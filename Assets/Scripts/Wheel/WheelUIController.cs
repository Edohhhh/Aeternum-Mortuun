using UnityEngine;
using System.Collections.Generic;

namespace EasyUI.PickerWheelUI
{
    public class WheelUIController : MonoBehaviour
    {
        [Header("Canvas de las ruletas")]
        [SerializeField] private List<GameObject> wheelCanvases;

        [Tooltip("¿Las ruletas deben estar activas al iniciar el juego?")]
        [SerializeField] private bool startActive = false;

        [Header("Referencia al selector de ruletas")]
        [SerializeField] private WheelSelector wheelSelector;

        private void Start()
        {
            foreach (var canvas in wheelCanvases)
            {
                if (canvas != null)
                    canvas.SetActive(startActive);
            }

            Time.timeScale = startActive ? 0f : 1f;
        }

        public void MostrarRuleta()
        {
            foreach (var canvas in wheelCanvases)
            {
                if (canvas != null)
                    canvas.SetActive(true);
            }

            Time.timeScale = 0f;
        }

        public void ConfirmarPremio()
        {
            foreach (var canvas in wheelCanvases)
            {
                if (canvas != null)
                    canvas.SetActive(false);
            }

            Time.timeScale = 1f;
        }

        public void VerificarYMostrarSiNoHayEnemigos()
        {
            GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemy");

            // Solo muestra si TODOS los canvas están desactivados
            bool todosDesactivados = wheelCanvases.TrueForAll(c => !c.activeSelf);

            if (enemigos.Length == 0 && todosDesactivados)
            {
                MostrarRuleta();
            }
        }
    }
}