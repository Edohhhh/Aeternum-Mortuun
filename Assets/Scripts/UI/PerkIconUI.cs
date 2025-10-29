using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // ¡Necesario para detectar el ratón!

public class PerkIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;

    private PowerUpEffect powerUpEffect;
    private InventoryTooltipUI tooltip;

    // 1. Inicializa este icono con los datos del ScriptableObject
    public void Initialize(PowerUpEffect effect, InventoryTooltipUI tooltipUI)
    {
        this.powerUpEffect = effect;
        this.tooltip = tooltipUI;

        if (iconImage == null)
            iconImage = GetComponent<Image>();

        iconImage.sprite = powerUpEffect.icon;
    }

    // 2. Cuando el ratón entra
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip != null && powerUpEffect != null)
        {
            tooltip.Show(powerUpEffect.label, powerUpEffect.description);
        }
    }

    // 3. Cuando el ratón sale
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
        {
            tooltip.Hide();
        }
    }
}