using UnityEngine;

public enum RockHardDamageMode { AddFlat, ExponentialSteps }

[CreateAssetMenu(fileName = "RockHardPowerUp", menuName = "PowerUps/RockHard!")]
public class RockHardPowerUp : PowerUp
{
    [Header("Daño")]
    public RockHardDamageMode damageMode = RockHardDamageMode.AddFlat;

    [Tooltip("Si damageMode = AddFlat, suma este valor al daño base.")]
    public float flatDamage = 5f;

    [Tooltip("Si damageMode = ExponentialSteps, multiplica: damage *= (expFactor ^ expSteps).")]
    public float expFactor = 1.15f;

    [Tooltip("Cantidad de pasos 'exponenciales' (por default 5 ≈ 2x con expFactor=1.15).")]
    public int expSteps = 5;

    [Header("Movimiento")]
    [Tooltip("Cuánto reduce la velocidad de movimiento (unidades).")]
    public float moveSpeedDelta = -3f;

    [Header("Dash")]
    [Tooltip("Multiplicador al cooldown del dash (1.5 = +50%).")]
    public float dashCooldownMultiplier = 1.5f;

    public override void Apply(PlayerController player)
    {
        // Buscar o crear el observer persistente
        var existing = GameObject.Find("RockHardObserver");
        RockHardObserver obs;
        if (existing == null)
        {
            var go = new GameObject("RockHardObserver");
            go.name = "RockHardObserver";
            obs = go.AddComponent<RockHardObserver>();
            Object.DontDestroyOnLoad(go);
        }
        else
        {
            obs = existing.GetComponent<RockHardObserver>();
        }

        // Pasar config y enlazar player
        obs.damageMode = damageMode;
        obs.flatDamage = flatDamage;
        obs.expFactor = expFactor;
        obs.expSteps = expSteps;
        obs.moveSpeedDelta = moveSpeedDelta;
        obs.dashCooldownMultiplier = dashCooldownMultiplier;

        obs.AttachToPlayer(player);
    }

    public override void Remove(PlayerController player)
    {
        // quitar la perk y revertir efectos:
        var go = GameObject.Find("RockHardObserver");
        if (go != null) Object.Destroy(go);
    }
}
