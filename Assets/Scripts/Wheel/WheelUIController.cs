using UnityEngine;
using System.Collections.Generic;
using TMPro; // ✅ AÑADIDO
using DG.Tweening; // ✅ AÑADIDO
using System.Collections; // ✅ AÑADIDO

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

        // ✅ --- AÑADIDO ---
        [Header("Feedback Visual")]
        [SerializeField] private TextMeshProUGUI floatingText;

        private Vector3 floatingTextOriginalPos;
        private Coroutine floatingTextCoroutine;
        // ✅ --- FIN ---

        private void Start()
        {
            foreach (var canvas in wheelCanvases)
            {
                if (canvas != null)
                    canvas.SetActive(startActive);
            }

            // ✅ --- AÑADIDO ---
            if (floatingText != null)
            {
                // Guardamos la posición inicial para resetear la animación
                if (floatingText.rectTransform != null)
                    floatingTextOriginalPos = floatingText.rectTransform.anchoredPosition;
                else
                    Debug.LogWarning("floatingText no tiene RectTransform. Asegúrate de que sea un objeto UI.");

                floatingText.gameObject.SetActive(false);
            }
            // ✅ --- FIN ---

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

            // ✅ --- AÑADIDO ---
            if (floatingText != null)
            {
                // Si había una animación anterior, la detenemos
                if (floatingTextCoroutine != null)
                    StopCoroutine(floatingTextCoroutine);

                // Iniciamos la nueva animación
                floatingTextCoroutine = StartCoroutine(AnimateFloatingText());
            }
            // ✅ --- FIN ---
        }

        // ✅ --- MÉTODO NUEVO AÑADIDO ---
        private IEnumerator AnimateFloatingText()
        {
            // Referencia al CanvasGroup (requerido en el objeto)
            CanvasGroup cg = floatingText.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                Debug.LogWarning("FloatingText no tiene CanvasGroup. Añádelo para el efecto de fade.");
                yield break;
            }

            // --- Resetear estado ---
            floatingText.gameObject.SetActive(true);
            cg.alpha = 0f;
            floatingText.rectTransform.anchoredPosition = floatingTextOriginalPos;
            floatingText.transform.localScale = Vector3.one * 0.9f; // Empezar un poco pequeño

            Vector3 endPos = floatingTextOriginalPos + new Vector3(0, 60, 0); // Hacia dónde se moverá

            // --- Crear secuencia de DOTween ---
            // Usamos SetUpdate(true) para que funcione con Time.timeScale = 0
            Sequence seq = DOTween.Sequence();
            seq.SetUpdate(true); // ¡IMPORTANTE!

            // 1. Aparecer (Fade In) y Escalar
            seq.Append(cg.DOFade(1f, 0.4f).SetEase(Ease.OutQuad));
            seq.Join(floatingText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack)); // Efecto "pop"

            // 2. Pausa (se mantiene visible)
            yield return new WaitForSecondsRealtime(1.5f);

            // 3. Desaparecer (Fade Out) y Moverse hacia arriba
            seq.Append(cg.DOFade(0f, 0.6f).SetEase(Ease.InQuad));
            seq.Join(floatingText.rectTransform.DOAnchorPos(endPos, 0.6f).SetEase(Ease.InQuad));

            // 4. Esperar a que termine la secuencia
            yield return seq.WaitForCompletion(true);

            // 5. Ocultar al finalizar
            floatingText.gameObject.SetActive(false);
            floatingTextCoroutine = null;
        }
        // ✅ --- FIN DEL MÉTODO NUEVO ---


        public void ConfirmarPremio()
        {
            foreach (var canvas in wheelCanvases)
            {
                if (canvas != null)
                    canvas.SetActive(false);
            }

            // ✅ --- AÑADIDO ---
            if (floatingTextCoroutine != null)
            {
                StopCoroutine(floatingTextCoroutine);
                floatingText.gameObject.SetActive(false);
            }
            // ✅ --- FIN ---

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