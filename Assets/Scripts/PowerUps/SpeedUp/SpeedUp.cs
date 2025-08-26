using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSpeedPowerUp", menuName = "PowerUps/Player Speed Up")]
public class PlayerSpeedPowerUp : PowerUp
{
    [Tooltip("Factor de aumento por pickup (1.15 = +15%)")]
    public float speedMultiplier = 1.15f;

    public override void Apply(PlayerController player)
    {
        // Multiplica la velocidad actual
        player.moveSpeed *= speedMultiplier;
    }

    public override void Remove(PlayerController player)
    {
        // Si querés revertir al perderlo:
        // player.moveSpeed /= speedMultiplier;
    }
}