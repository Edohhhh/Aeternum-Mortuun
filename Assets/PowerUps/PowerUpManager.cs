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
