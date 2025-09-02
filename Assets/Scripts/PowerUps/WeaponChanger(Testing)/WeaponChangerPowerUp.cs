using UnityEngine;

[CreateAssetMenu(fileName = "WeaponChangerPowerUp", menuName = "PowerUps/Weapon Changer")]
public class WeaponChangerPowerUp : PowerUp
{
    [Header("Configuración de Arma")]
    public GameObject[] newSlashEffectPrefabs; // 3 prefabs para los 3 golpes del combo
    public GameObject newHitboxPrefab;         // Nuevo prefab de hitbox

    [Header("Configuración de Combate")]
    public float newAttackCooldown = 0.3f;
    public float newHitboxOffset = 0.5f;

    // 🔄 Recoil del CombatSystem simplificado (distancia + duración)
    [Header("Recoil (CombatSystem simplificado)")]
    public float newRecoilDistance = 0.22f;  // cuánto avanza al atacar
    public float newRecoilDuration = 0.07f;  // cuánto dura ese empujón antes de frenar en seco

    public override void Apply(PlayerController player)
    {
        var combatSystem = player.GetComponent<CombatSystem>();
        if (combatSystem == null)
        {
            Debug.LogError("[WEAPON CHANGER] No se encontró el CombatSystem en el jugador");
            return;
        }

        // Cambiar prefabs visuales
        if (newSlashEffectPrefabs != null && newSlashEffectPrefabs.Length > 0)
            combatSystem.slashEffectPrefabs = newSlashEffectPrefabs;

        // Cambiar prefab de hitbox
        if (newHitboxPrefab != null)
            combatSystem.hitboxPrefab = newHitboxPrefab;

        // Cambiar configuraciones de combate
        combatSystem.attackCooldown = newAttackCooldown;
        combatSystem.hitboxOffset = newHitboxOffset;

        // ✅ Ajustar recoil del sistema nuevo
        combatSystem.recoilDistance = newRecoilDistance;
        combatSystem.recoilDuration = newRecoilDuration;

        Debug.Log("[WEAPON CHANGER] Arma cambiada exitosamente");
        Debug.Log($"Nuevos parámetros - Cooldown: {newAttackCooldown}, Offset: {newHitboxOffset}, RecoilDistance: {newRecoilDistance}, RecoilDuration: {newRecoilDuration}");
    }

    public override void Remove(PlayerController player)
    {
        // Si más adelante querés restaurar valores originales, armamos un snapshot por jugador.
        Debug.Log("[WEAPON CHANGER] Power Up removido");
    }
}
