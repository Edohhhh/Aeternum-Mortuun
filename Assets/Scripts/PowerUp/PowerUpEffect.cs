using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ruleta/PowerUpEffect")]
public class PowerUpEffect : ScriptableObject
{
    [Header("Visual")]
    public string label;
    public Sprite icon;

    [Header("Efecto")]
    public PowerUp powerUp; // Referencia al ScriptableObject funcional

    public void Apply(GameObject playerObj)
    {
        if (playerObj == null)
        {
            Debug.LogError("❌ Player GameObject nulo.");
            return;
        }

        var controller = playerObj.GetComponent<PlayerController>();
        if (controller == null)
        {
            Debug.LogError("❌ PlayerController no encontrado en el objeto.");
            return;
        }

        if (powerUp == null)
        {
            Debug.LogWarning($"⚠️ PowerUp no asignado en PowerUpEffect '{label}'");
            return;
        }

        // Aplicar el efecto
        powerUp.Apply(controller);

        // Añadir el PowerUp a la lista si no estaba
        if (controller.initialPowerUps == null)
            controller.initialPowerUps = new PowerUp[0];

        var list = new List<PowerUp>(controller.initialPowerUps);
        if (!list.Contains(powerUp))
        {
            list.Add(powerUp);
            controller.initialPowerUps = list.ToArray();
            Debug.Log($"✅ PowerUp '{powerUp.name}' añadido a initialPowerUps.");
        }
        else
        {
            Debug.Log($"ℹ️ PowerUp '{powerUp.name}' ya estaba asignado.");
        }

        // Asociar este PowerUpEffect al PowerUp para la UI
        if (powerUp.effect == null)
            powerUp.effect = this;

        // Refrescar la UI si existe
        var ui = Object.FindObjectOfType<PowerUpUI>();
        if (ui != null)
        {
            ui.RefreshUI();
        }
    }
}
