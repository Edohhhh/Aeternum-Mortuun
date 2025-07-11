using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager
{
    private PlayerController player;
    private List<PowerUpInstance> activePowerUps = new();

    public PowerUpManager(PlayerController player)
    {
        this.player = player;
    }

    public void AddPowerUp(PowerUp powerUp)
    {
        powerUp.Apply(player);
        if (!powerUp.isPermanent)
            activePowerUps.Add(new PowerUpInstance(powerUp));
    }

    public void ActivarPowerUp(string id)
    {
        switch (id)
        {
            case "AcidDash":
                ActivarDashAcido();
                break;

            // Agregá más casos si tenés otros powerups únicos

            default:
                Debug.LogWarning($"PowerUpManager no reconoce el id: {id}");
                break;
        }
    }

    private void ActivarDashAcido()
    {
        
        Debug.Log("Activado efecto Dash Ácido en PowerUpManager.");
    }

    public void Update(float deltaTime)
    {
        for (int i = activePowerUps.Count - 1; i >= 0; i--)
        {
            var instance = activePowerUps[i];
            instance.Update(player, deltaTime);

            if (instance.IsExpired)
            {
                activePowerUps.RemoveAt(i);
            }
        }
    }
}
