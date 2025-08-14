using UnityEngine;

public class BloodRitualHook : MonoBehaviour
{
    private EnemyHealth health;
    private PlayerController player;
    private float lastHealth;

    private float healChance;
    private float healAmount;

    private float recentPlayerHitTimer = 0f;
    private const float detectionWindow = 0.1f;

    public void Initialize(EnemyHealth enemyHealth, PlayerController playerController, float chance, float amount)
    {
        health = enemyHealth;
        player = playerController;
        healChance = chance;
        healAmount = amount;
        lastHealth = health.GetCurrentHealth();
    }

    private void Update()
    {
        if (health == null || player == null) return;

        if (recentPlayerHitTimer > 0f)
            recentPlayerHitTimer -= Time.deltaTime;

        float current = health.GetCurrentHealth();

        if (current < lastHealth && recentPlayerHitTimer > 0f)
        {
            if (Random.value < healChance)
            {
                var playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.ModifyHealthFlat(healAmount);
                }
            }

            recentPlayerHitTimer = 0f;
        }

        lastHealth = current;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            recentPlayerHitTimer = detectionWindow;
        }
    }
}
