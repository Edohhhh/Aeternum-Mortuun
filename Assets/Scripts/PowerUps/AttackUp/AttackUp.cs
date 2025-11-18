using UnityEngine;

[CreateAssetMenu(fileName = "AttackUp", menuName = "PowerUps/Damage Up +15%")]
public class AttackUp : PowerUp
{
    [Tooltip("Multiplicador por pickup (1.15 = +15%)")]
    public float damageMultiplier = 1.15f;

    public override void Apply(PlayerController player)
    {
        if (player == null) return;

        int before = Mathf.Max(1, player.baseDamage);

        int after = Mathf.CeilToInt(before * damageMultiplier);

        if (after <= before) after = before + 1;

        player.baseDamage = after;

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
