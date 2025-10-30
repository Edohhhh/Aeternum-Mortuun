using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class UIFadeInFromBottom : MonoBehaviour
{
    [Header("Animación de entrada")]
    public float moveDistance = 300f;       // Distancia desde abajo
    public float moveDuration = 0.6f;       // Duración del movimiento
    public float fadeDuration = 0.6f;       // Duración del fade
    public Ease moveEase = Ease.OutCubic;   // Tipo de easing del movimiento
    public Ease fadeEase = Ease.OutQuad;    // Tipo de easing del fade
    public float startDelay = 0f;           // Retraso antes de iniciar la animación

    RectTransform rect;
    CanvasGroup canvasGroup;
    Vector2 startPos;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        // Guardamos posición original
        startPos = rect.anchoredPosition;

        // Colocar fuera de la pantalla y transparente
        rect.anchoredPosition = startPos - new Vector2(0, moveDistance);
        canvasGroup.alpha = 0f;

        // Crear animaciones combinadas
        Sequence seq = DOTween.Sequence();

        // Mover hacia su posición original
        seq.Append(rect.DOAnchorPos(startPos, moveDuration)
                       .SetEase(moveEase)
                       .SetDelay(startDelay));

        // Hacer Fade In
        seq.Join(canvasGroup.DOFade(1f, fadeDuration)
                            .SetEase(fadeEase)
                            .SetDelay(startDelay));

        seq.Play();
    }
}
