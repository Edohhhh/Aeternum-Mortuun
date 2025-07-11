using UnityEngine;

[CreateAssetMenu(fileName = "LifeUpPowerUp", menuName = "PowerUps/Life Up")]
public class LifeUpPowerUp : PowerUp
{
    public float extraMaxHealth = 0.5f;

    public override void Apply(PlayerController player)
    {
        var health = player.GetComponent<PlayerHealth>();
        if (health == null) return;

        // Aumentar vida
        health.maxHealth += extraMaxHealth;
        health.currentHealth = Mathf.Min(health.currentHealth + extraMaxHealth, health.maxHealth);

        // Forzar reinicio de la UI de corazones
        if (health.healthUI != null)
            health.healthUI.Initialize(health.maxHealth);
    }

    public override void Remove(PlayerController player)
    {
        // Nada por ahora
    }
}
