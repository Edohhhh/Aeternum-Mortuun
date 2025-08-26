using UnityEngine;

[CreateAssetMenu(fileName = "WeaponChangerPowerUp", menuName = "PowerUps/Weapon Changer")]
public class WeaponChangerPowerUp : PowerUp
{
    [Header("Configuración de Arma")]
    public GameObject[] newSlashEffectPrefabs; // 3 prefabs para los 3 golpes del combo
    public GameObject newHitboxPrefab; // Nuevo prefab de hitbox

    [Header("Configuración de Combate")]
    public float newAttackCooldown = 0.3f;
    public float newHitboxOffset = 0.5f;
    public float newDashSpeed = 150f;

    public override void Apply(PlayerController player)
    {
        CombatSystem combatSystem = player.GetComponent<CombatSystem>();
        if (combatSystem != null)
        {
            // Cambiar los prefabs de efectos visuales
            if (newSlashEffectPrefabs != null && newSlashEffectPrefabs.Length > 0)
            {
                combatSystem.slashEffectPrefabs = newSlashEffectPrefabs;
            }

            // Cambiar el prefab de hitbox
            if (newHitboxPrefab != null)
            {
                combatSystem.hitboxPrefab = newHitboxPrefab;
            }

            // Cambiar configuraciones de combate
            combatSystem.attackCooldown = newAttackCooldown;
            combatSystem.hitboxOffset = newHitboxOffset;
            combatSystem.dashSpeed = newDashSpeed;

            Debug.Log($"[WEAPON CHANGER] Arma cambiada exitosamente");
            Debug.Log($"Nuevos parámetros - Cooldown: {newAttackCooldown}, Offset: {newHitboxOffset}, DashSpeed: {newDashSpeed}");
        }
        else
        {
            Debug.LogError("[WEAPON CHANGER] No se encontró el CombatSystem en el jugador");
        }
    }

    public override void Remove(PlayerController player)
    {
        // Opcional: Podrías restaurar los valores originales aquí
        Debug.Log("[WEAPON CHANGER] Power Up removido");
    }
}