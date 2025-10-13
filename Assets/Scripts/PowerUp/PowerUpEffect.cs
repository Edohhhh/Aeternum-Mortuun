using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ruleta/PowerUpEffect")]
public class PowerUpEffect : ScriptableObject
{
    [Header("Visual")]
    public string label;
    [TextArea(2, 4)] public string description;
    public Sprite icon;

    [Header("Efecto")]
    public PowerUp powerUp; 

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

       
        powerUp.Apply(controller);

        
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

   
        if (powerUp.effect == null)
            powerUp.effect = this;


        var ui = Object.FindObjectOfType<PowerUpUI>();
        if (ui != null)
        {
            ui.RefreshUI();
        }
    }
}
