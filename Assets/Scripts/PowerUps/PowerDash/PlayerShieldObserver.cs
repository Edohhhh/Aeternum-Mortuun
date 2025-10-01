using UnityEngine;

public class PlayerShieldObserver : MonoBehaviour
{
    public PlayerHealth playerHealth;
    private ShieldEffect shield;

    private float lastHealth;

    void Start()
    {
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        shield = GetComponent<ShieldEffect>();
        if (playerHealth != null)
            lastHealth = playerHealth.currentHealth;
    }

    void Update()
    {
        if (playerHealth == null) return;

        // Si el escudo está activo y la salud bajó, revertimos
        if (shield != null && shield.IsShieldActive())
        {
            if (playerHealth.currentHealth < lastHealth)
            {
                // revertimos la resta de vida
                float diff = lastHealth - playerHealth.currentHealth;
                playerHealth.currentHealth += diff;
              

                // restaurar UI
                playerHealth.UpdateUI();
            }
        }

        lastHealth = playerHealth.currentHealth;
    }
}
