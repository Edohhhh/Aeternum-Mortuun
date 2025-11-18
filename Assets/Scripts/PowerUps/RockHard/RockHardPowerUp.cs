using UnityEngine;
using System.Reflection;

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

    // Estado para revertir cooldown
    private object dashHost;
    private FieldInfo dashCooldownField;
    private float originalDashCooldown;
    private bool dashPatched;

    public override void Apply(PlayerController player)
    {
        if (player == null) return;

        dashPatched = false;
        dashHost = null;
        dashCooldownField = null;

        // ====== DAÑO ======
        if (damageMode == RockHardDamageMode.AddFlat)
        {
            int add = Mathf.RoundToInt(flatDamage);
            player.baseDamage += add;
        }
        else
        {
            float mult = Mathf.Pow(expFactor, Mathf.Max(0, expSteps));
            player.baseDamage = Mathf.RoundToInt(player.baseDamage * mult);
        }

        // ====== MOVIMIENTO ======
        player.moveSpeed = Mathf.Max(0f, player.moveSpeed + moveSpeedDelta);

        // ====== DASH COOLDOWN (por reflexión) ======
        PatchDashCooldown(player, dashCooldownMultiplier);
    }

    public override void Remove(PlayerController player)
    {
        if (player == null) return;

        // Inverso de lo que hicimos en Apply:

        if (damageMode == RockHardDamageMode.AddFlat)
        {
            int add = Mathf.RoundToInt(flatDamage);
            player.baseDamage -= add;
        }
        else
        {
            float mult = Mathf.Pow(expFactor, Mathf.Max(0, expSteps));
            float invMult = 1f / mult;
            player.baseDamage = Mathf.RoundToInt(player.baseDamage * invMult);
        }

        player.moveSpeed = player.moveSpeed - moveSpeedDelta; // inverso: si sumé -3, ahora resto -3 (= +3)

        if (dashPatched && dashHost != null && dashCooldownField != null)
        {
            dashCooldownField.SetValue(dashHost, originalDashCooldown);
        }

        dashPatched = false;
        dashHost = null;
        dashCooldownField = null;
    }

    private void PatchDashCooldown(PlayerController player, float multiplier)
    {
        string[] names = { "dashCooldown", "DashCooldown", "dashCd", "dashDelay", "cooldown" };
        var comps = player.GetComponents<MonoBehaviour>();

        foreach (var c in comps)
        {
            if (c == null) continue;
            var t = c.GetType();

            foreach (var n in names)
            {
                var f = t.GetField(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (f != null && f.FieldType == typeof(float))
                {
                    dashHost = c;
                    dashCooldownField = f;

                    originalDashCooldown = (float)dashCooldownField.GetValue(dashHost);
                    float newCd = originalDashCooldown * Mathf.Max(0.01f, multiplier);
                    dashCooldownField.SetValue(dashHost, newCd);

                    dashPatched = true;
                    return;
                }
            }
        }
    }
}
