using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSpeedPowerUp", menuName = "PowerUps/Player Speed Up")]
public class PlayerSpeedPowerUp : PowerUp
{
    [Tooltip("Factor de aumento por pickup (1.15 = +15%)")]
    public float speedMultiplier = 1.15f;

    public override void Apply(PlayerController player)
    {
        if (player == null) return;

        // Multiplica la velocidad actual
        player.moveSpeed *= speedMultiplier;

        
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.SavePlayerData(player);
        }

       
        BeggarValueObserver.RequestReapply();
    }

    public override void Remove(PlayerController player)
    {
        // BeggarValueObserver.RequestReapply();
    }
}
