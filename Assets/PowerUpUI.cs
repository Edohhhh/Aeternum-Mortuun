using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpUI : MonoBehaviour
{
    public GameObject iconPrefab; // Prefab con Image y LayoutElement
    public Transform container;   // Contenedor con VerticalLayoutGroup

    private PlayerController player;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        RefreshUI();
    }

    public void RefreshUI()
    {
        // Limpiar íconos anteriores
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        // Mostrar los íconos de los PowerUps activos
        foreach (var powerUp in player.initialPowerUps)
        {
            if (powerUp != null && powerUp.effect != null && powerUp.effect.icon != null)
            {
                var iconGO = Instantiate(iconPrefab, container);
                iconGO.GetComponent<Image>().sprite = powerUp.effect.icon;

                // Si querés agregar texto, lo buscás así:
                // var text = iconGO.GetComponentInChildren<Text>();
                // if (text != null) text.text = powerUp.effect.effectName;
            }
        }
    }
}