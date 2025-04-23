using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;
    private int currentHealth;

    private EnemyKnockback enemyKnockback;

    [Header("Effects")]
    public ParticleSystem bloodEffect; // Referencia al sistema de partículas

    void Start()
    {
        currentHealth = maxHealth;
        enemyKnockback = GetComponent<EnemyKnockback>();

        if (enemyKnockback == null)
        {
            Debug.LogError("EnemyKnockback script is missing.");
        }

        if (bloodEffect == null)
        {
            Debug.LogWarning("Blood particle system is not assigned.");
        }
    }

    public void TakeDamage(int damage, Vector2 knockbackSourcePosition)
    {
        currentHealth -= damage;

        // Reproduce el efecto de sangre
        if (bloodEffect != null)
        {
            // Posiciona la sangre donde fue golpeado el enemigo (opcional)
            bloodEffect.transform.position = transform.position;
            bloodEffect.Play();
        }

        if (enemyKnockback != null)
        {
            Vector2 knockbackDirection = (transform.position - (Vector3)knockbackSourcePosition).normalized;
            enemyKnockback.ApplyKnockback(knockbackDirection);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
