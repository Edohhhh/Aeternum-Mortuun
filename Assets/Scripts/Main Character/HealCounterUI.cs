using UnityEngine;
using TMPro;

public class HealCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI counterText;

    [Header("Opcionales")]
    [SerializeField] private string prefix = ""; // e.g. "Q: "
    [SerializeField] private string suffix = ""; // e.g. " restantes"
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color emptyColor = Color.gray;

    public void SetHealsRemaining(int current, int max)
    {
        if (counterText == null) return;

        counterText.text = $"{prefix}{current}/{max}{suffix}";
        counterText.color = (current <= 0) ? emptyColor : normalColor;
    }

    // Método público para forzar una comprobación (útil si quieres refrescar manualmente desde editor)
    public void Refresh(int current, int max) => SetHealsRemaining(current, max);
}
