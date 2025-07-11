using UnityEngine;

public class PowerUpInstance
{
    public PowerUp powerUp;
    public float timer;

    public PowerUpInstance(PowerUp powerUp)
    {
        this.powerUp = powerUp;
        this.timer = powerUp.duration;
    }

    public void Update(PlayerController player, float deltaTime)
    {
        if (!powerUp.isPermanent)
        {
            timer -= deltaTime;
            if (timer <= 0f)
            {
                powerUp.Remove(player);
            }
        }
    }

    public bool IsExpired => !powerUp.isPermanent && timer <= 0f;
}
