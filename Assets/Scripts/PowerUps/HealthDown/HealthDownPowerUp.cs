using UnityEngine;

[CreateAssetMenu(fileName = "HealthDownPowerUp", menuName = "PowerUps/Health Down")]
public class HealthDownPowerUp : PowerUp
{
    [Header("Configuración")]
    public float healthReduction = 1f; // Reducir 1 corazón

    public override void Apply(PlayerController player)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.ModifyHealthFlat(-healthReduction);
            Debug.Log($"[HEALTH DOWN] Vida reducida en {healthReduction} unidades");
        }
    }

    public override void Remove(PlayerController player)
    {
        // No requiere acción al remover
        Debug.Log("[HEALTH DOWN] Power Up removido");
    }
}