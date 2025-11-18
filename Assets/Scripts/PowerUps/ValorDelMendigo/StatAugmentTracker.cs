using UnityEngine;

[DisallowMultipleComponent]
public class StatAugmentTracker : MonoBehaviour
{
    private int appliedDamage;
    private float appliedMoveSpeed;
    private float appliedMaxHealth;
    private float appliedMultiplier = 1f;

    public void ApplyTo(PlayerController player, int addDamage, float addMoveSpeed, float addMaxHealth, float moveSpeedMultiplier)
    {
        // Aplicar daño
        player.baseDamage = (int)(player.baseDamage + addDamage);
        appliedDamage = addDamage;

        // Aplicar velocidad (sumatoria y multiplicador)
        player.moveSpeed = (player.moveSpeed + addMoveSpeed) * moveSpeedMultiplier;
        appliedMoveSpeed = addMoveSpeed;
        appliedMultiplier = moveSpeedMultiplier;

        // Aplicar vida máxima
        var ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.maxHealth += addMaxHealth;
            ph.currentHealth = Mathf.Min(ph.currentHealth + addMaxHealth, ph.maxHealth);
            appliedMaxHealth = addMaxHealth;

            if (ph.healthUI != null)
            {
                ph.healthUI.Initialize(ph.maxHealth);
                ph.healthUI.UpdateHearts(ph.currentHealth);
            }
        }
    }

    public void RemoveFrom(PlayerController player)
    {
        player.baseDamage = (int)(player.baseDamage - appliedDamage);

        player.moveSpeed = (player.moveSpeed - appliedMoveSpeed) / appliedMultiplier;

        var ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.maxHealth -= appliedMaxHealth;
            ph.currentHealth = Mathf.Min(ph.currentHealth, ph.maxHealth);

            if (ph.healthUI != null)
            {
                ph.healthUI.Initialize(ph.maxHealth);
                ph.healthUI.UpdateHearts(ph.currentHealth);
            }
        }

        appliedDamage = 0;
        appliedMoveSpeed = 0f;
        appliedMaxHealth = 0f;
        appliedMultiplier = 1f;
    }
}
