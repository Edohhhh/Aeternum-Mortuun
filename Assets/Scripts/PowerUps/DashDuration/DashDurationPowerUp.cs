using UnityEngine;

[CreateAssetMenu(fileName = "DashDurationPowerUp", menuName = "PowerUps/Dash Duration Up")]
public class DashDurationPowerUp : PowerUp
{
    [Tooltip("Factor de aumento por pickup (1.2 = +20%)")]
    public float durationMultiplier = 1.2f;

    public override void Apply(PlayerController player)
    {
        // Solo multiplicamos la duración actual
        player.dashDuration *= durationMultiplier;
    }

    public override void Remove(PlayerController player)
    {
        // Si querés revertir cuando se pierde el powerup:
        // player.dashDuration /= durationMultiplier;
    }
}
