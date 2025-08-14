using UnityEngine;

[CreateAssetMenu(fileName = "FeatherweightRuntimePowerUp", menuName = "PowerUps/Featherweight Runtime")]
public class FeatherweightRuntimePowerUp : PowerUp
{
    public float speedBonus = 1f;
    public FeatherweightRuntimePowerUp runtimePowerUp;

    public override void Apply(PlayerController player)
    {

        player.moveSpeed += speedBonus;
        RemoveSelfFromPlayer(player);
    }

    void Start()
    {
        PlayerController player = Object.FindAnyObjectByType<PlayerController>();
        if (runtimePowerUp != null)
            runtimePowerUp.Apply(player);
    }

    public override void Remove(PlayerController player)
    {
        
    }

    private void RemoveSelfFromPlayer(PlayerController player)
    {
        if (player.initialPowerUps == null) return;

        var list = new System.Collections.Generic.List<PowerUp>(player.initialPowerUps);

        if (list.Contains(this))
        {
            list.Remove(this);
            player.initialPowerUps = list.ToArray();
        }
    }
}
