using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class UIDialogueAnimator : MonoBehaviour
{
    [Header("Entrada")]
    public float moveDistance = 200f;
    public float moveDuration = 0.6f;
    public float fadeDuration = 0.6f;
    public Ease moveEase = Ease.OutCubic;
    public Ease fadeEase = Ease.OutQuad;

    [Header("Salida")]
    public float exitMoveDistance = 200f;
    public float exitDuration = 0.5f;
    public float fadeOutDuration = 0.4f;
    public Ease exitEase = Ease.InCubic;
    public Ease fadeOutEase = Ease.InQuad;

    RectTransform rect;
    CanvasGroup canvasGroup;
    Vector2 originalPos;
    Sequence currentSeq;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        originalPos = rect.anchoredPosition;
    }

    void OnEnable()
    {
        AnimateIn();
    }

    void OnDisable()
    {
        // Si se desactiva abruptamente, matamos cualquier tween pendiente
        currentSeq?.Kill();
    }

    public void AnimateIn()
    {
        currentSeq?.Kill();

        rect.anchoredPosition = originalPos - new Vector2(0, moveDistance);
        canvasGroup.alpha = 0f;

        currentSeq = DOTween.Sequence();
        currentSeq.Append(rect.DOAnchorPos(originalPos, moveDuration).SetEase(moveEase));
        currentSeq.Join(canvasGroup.DOFade(1f, fadeDuration).SetEase(fadeEase));
    }

    public void AnimateOut(System.Action onComplete = null)
    {
        currentSeq?.Kill();

        currentSeq = DOTween.Sequence();
        currentSeq.Append(rect.DOAnchorPos(originalPos - new Vector2(0, exitMoveDistance), exitDuration)
                              .SetEase(exitEase));
        currentSeq.Join(canvasGroup.DOFade(0f, fadeOutDuration).SetEase(fadeOutEase));
        currentSeq.OnComplete(() =>
        {
            onComplete?.Invoke();
            gameObject.SetActive(false); // ocultar panel al final
        });
    }
}
