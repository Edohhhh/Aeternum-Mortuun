using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class RewardPopupUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private CanvasGroup popupCanvas;
    [SerializeField] private Image rewardImage;
    [SerializeField] private TextMeshProUGUI rewardName;
    [SerializeField] private TextMeshProUGUI rewardDescription;

    [Header("Configuración")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float autoHideTime = 3f;

    private Tween fadeTween;

    private void Awake()
    {
        popupCanvas.alpha = 0;
        popupCanvas.interactable = false;
        popupCanvas.blocksRaycasts = false;
    }

    public void ShowReward(Sprite sprite, string name, string description)
    {
        rewardImage.sprite = sprite;
        rewardName.text = name;
        rewardDescription.text = description;

        popupCanvas.interactable = true;
        popupCanvas.blocksRaycasts = true;
        fadeTween?.Kill();
        fadeTween = popupCanvas.DOFade(1f, fadeDuration);

        // Ocultar automáticamente después de unos segundos
        CancelInvoke(nameof(Hide));
        Invoke(nameof(Hide), autoHideTime);
    }

    public void Hide()
    {
        fadeTween?.Kill();
        fadeTween = popupCanvas.DOFade(0f, fadeDuration)
            .OnComplete(() =>
            {
                popupCanvas.interactable = false;
                popupCanvas.blocksRaycasts = false;
            });
    }
}
