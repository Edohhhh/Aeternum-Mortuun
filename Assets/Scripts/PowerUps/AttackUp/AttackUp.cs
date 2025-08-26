using UnityEngine;
[CreateAssetMenu(fileName = "AttackUp", menuName = "PowerUps/Damage Up +15%")]
public class AttackUp : PowerUp
{
    [Tooltip("Multiplicador por pickup (1.15 = +15%)")]
    public float damageMultiplier = 1.15f;

    public override void Apply(PlayerController player)
    {
        if (player == null) return;

        // Valor previo (m�nimo 1 para evitar quedarse en 0)
        int before = Mathf.Max(1, player.baseDamage);

        // Escalado + redondeo hacia arriba para no perder el 15% por truncado
        int after = Mathf.CeilToInt(before * damageMultiplier);

        // Red de seguridad: asegur� al menos +1 si por alguna raz�n no subi�
        if (after <= before) after = before + 1;

        player.baseDamage = after;

        // Persistir inmediatamente para que se mantenga entre escenas
        GameDataManager.Instance.SavePlayerData(player);
    }

    public override void Remove(PlayerController player)
    {
        // Opcional (si quer�s revertir):
        // int restored = Mathf.Max(1, Mathf.RoundToInt(player.baseDamage / damageMultiplier));
        // player.baseDamage = restored;
        // GameDataManager.Instance.SavePlayerData(player);
    }
}