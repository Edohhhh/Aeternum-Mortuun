using UnityEngine;
using System.Collections.Generic;
using TMPro;
using DG.Tweening; // ¡DOTween es necesario!
using System.Collections;
using System.Linq; // Necesario para 'TrueForAll'

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

        [Header("Feedback Visual (Popups)")]
        [SerializeField] private TextMeshProUGUI floatingText;

        // --- Texto de Instrucción "Seleccione..." ---
        [Header("Feedback Visual (Instrucción)")]
        [SerializeField] private TextMeshProUGUI textoInstruccion; // 👈 Asegúrate de que tu texto esté arrastrado aquí
        private CanvasGroup instruccionCanvasGroup;
        private Sequence instruccionBreathingAnim;
        // --- Fin ---

        private Vector3 floatingTextOriginalPos;
        private Coroutine floatingTextCoroutine;

        private void Start()
        {
            foreach (var canvas in wheelCanvases)
            {
                if (canvas != null)
                    canvas.SetActive(startActive);
            }

            if (floatingText != null)
            {
                if (floatingText.rectTransform != null)
                    floatingTextOriginalPos = floatingText.rectTransform.anchoredPosition;
                else
                    Debug.LogWarning("floatingText no tiene RectTransform.");
            }

            // --- Lógica para el texto de instrucción ---
            if (textoInstruccion != null)
            {
                // 1. Obtener o añadir el CanvasGroup
                instruccionCanvasGroup = textoInstruccion.GetComponent<CanvasGroup>();
                if (instruccionCanvasGroup == null)
                {
                    Debug.LogWarning("Texto de Instrucción no tenía CanvasGroup. Añadiendo uno...");
                    instruccionCanvasGroup = textoInstruccion.gameObject.AddComponent<CanvasGroup>();
                }

                instruccionCanvasGroup.alpha = 1f;
                textoInstruccion.gameObject.SetActive(true);

                // 2. Crear la animación de respiración
                instruccionBreathingAnim = DOTween.Sequence();
                instruccionBreathingAnim.Append(instruccionCanvasGroup.DOFade(0.7f, 1.5f).SetEase(Ease.InOutSine));
                instruccionBreathingAnim.Append(instruccionCanvasGroup.DOFade(1.0f, 1.5f).SetEase(Ease.InOutSine));
                instruccionBreathingAnim.SetLoops(-1); // Repetir por siempre

                // ✅ --- ¡ARREGLO 1! ---
                // Le decimos a TODA la secuencia que ignore la pausa del juego
                instruccionBreathingAnim.SetUpdate(true);
                // ✅ --- FIN ---
            }
            else
            {
                Debug.LogWarning("WheelUIController: 'Texto Instruccion' no está asignado en el Inspector.");
            }
            // --- Fin ---
        }

        /// <summary>
        /// Oculta el texto de instrucción con un fade-out.
        /// </summary>
        public void OcultarTextoInstruccion()
        {
            if (instruccionCanvasGroup == null) return;

            if (instruccionBreathingAnim != null && instruccionBreathingAnim.IsActive())
                instruccionBreathingAnim.Kill();

            instruccionCanvasGroup.DOFade(0f, 0.5f) // Fade rápido de 0.5 seg
                .SetEase(Ease.OutQuad)

                // ✅ --- ¡ARREGLO 2! ---
                // Le decimos a ESTA animación que ignore la pausa
                .SetUpdate(true)
                // ✅ --- FIN ---

                .OnComplete(() => {
                    if (textoInstruccion != null)
                        textoInstruccion.gameObject.SetActive(false);
                });
        }


        // (Aquí iría tu Coroutine ShowFloatingText si la tienes)


        public void ConfirmarPremio()
        {
            foreach (var canvas in wheelCanvases)
            {
                if (canvas != null)
                    canvas.SetActive(false);
            }

            OcultarTextoInstruccion();

            if (floatingTextCoroutine != null)
            {
                StopCoroutine(floatingTextCoroutine);
                floatingText.gameObject.SetActive(false);
            }

            // Reanudamos el juego
            Time.timeScale = 1f;
        }

        /// <summary>
        /// Muestra forzadamente la UI de la ruleta, inicializa el selector y pausa el juego.
        /// </summary>
        public void MostrarRuleta()
        {
            foreach (var canvas in wheelCanvases)
            {
                if (canvas != null)
                    canvas.SetActive(true);
            }

            if (wheelSelector != null)
                wheelSelector.IniciarSelector();

            // Pausamos el juego
            Time.timeScale = 0f;

            // Reactivar el texto de instrucción
            if (textoInstruccion != null && instruccionCanvasGroup != null)
            {
                instruccionCanvasGroup.alpha = 1f;
                textoInstruccion.gameObject.SetActive(true);
                if (instruccionBreathingAnim != null)
                    instruccionBreathingAnim.Play(); // Reanuda la animación de respiración
            }
        }

        /// <summary>
        /// Comprueba si no hay enemigos y si la ruleta no está ya mostrada.
        /// </summary>
        public void VerificarYMostrarSiNoHayEnemigos()
        {
            GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemy");

            bool todosDesactivados = wheelCanvases.TrueForAll(c => c == null || !c.activeSelf);

            if (enemigos.Length == 0 && todosDesactivados)
            {
                MostrarRuleta();
            }
        }
    }
}