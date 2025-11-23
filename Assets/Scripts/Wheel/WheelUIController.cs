using UnityEngine;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

namespace EasyUI.PickerWheelUI
{
    public class WheelUIController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private List<GameObject> wheelCanvases;
        [SerializeField] private WheelSelector wheelSelector;
        [SerializeField] private TextMeshProUGUI textoInstruccion;

        private void Start()
        {
            foreach (var canvas in wheelCanvases)
                if (canvas != null) canvas.SetActive(false);

            if (textoInstruccion != null) textoInstruccion.gameObject.SetActive(false);
        }

        public void MostrarRuleta()
        {
            var combat = FindObjectOfType<CombatSystem>();
            if (combat != null) combat.ForceStopCombatForUI();

            foreach (var canvas in wheelCanvases) if (canvas != null) canvas.SetActive(true);
            if (wheelSelector != null) wheelSelector.IniciarSelector();

            Time.timeScale = 0f;

            if (textoInstruccion != null)
            {
                textoInstruccion.text = "Selecciona una ruleta";
                textoInstruccion.gameObject.SetActive(true);
            }
        }

        // ✅ APAGADO INSTANTÁNEO
        public void OcultarTodo()
        {
            foreach (var canvas in wheelCanvases) if (canvas != null) canvas.SetActive(false);
            if (textoInstruccion != null) textoInstruccion.gameObject.SetActive(false);
        }

        public void ActualizarInstruccion(string texto)
        {
            if (textoInstruccion != null) textoInstruccion.text = texto;
        }

        public void VerificarYMostrarSiNoHayEnemigos()
        {
            if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
            {
                bool yaMostrada = wheelCanvases.Exists(c => c != null && c.activeSelf);
                if (!yaMostrada) MostrarRuleta();
            }
        }
    }
}
