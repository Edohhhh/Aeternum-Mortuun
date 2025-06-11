using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    public delegate void OnDeathDelegate();
    public event OnDeathDelegate OnDeath;
    public event System.Action OnDamaged;

    private Rigidbody2D rb;
    private float baseDamping = 5f;
    private float hitDamping = 10f;
    private float stunTimer;

    private void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearDamping = baseDamping;
            rb.gravityScale = 1f; // Asegura que haya gravedad si es necesario
        }
    }

    private void Update()
    {
        if (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f && rb != null)
            {
                rb.linearDamping = baseDamping;
            }
        }
    }

    /// <summary>
    /// Aplica daño al enemigo, retroceso y efectos de aturdimiento.
    /// </summary>
    public void TakeDamage(float damage, Vector2 knockbackDir, float knockbackForce)
    {
        if (currentHealth <= 0f)
            return;

        currentHealth -= damage;
        OnDamaged?.Invoke();

        if (rb != null)
        {
            // Resetear velocidad antes de aplicar knockback
            rb.linearVelocity = Vector2.zero;

            // Aplicar knockback directamente a la velocidad
            Vector2 knockbackVelocity = knockbackDir.normalized * knockbackForce;
            rb.linearVelocity = knockbackVelocity;

            // Ajustar damping para simular desaceleración
            rb.linearDamping = hitDamping;

            // Duración breve de aturdimiento antes de volver a damping base
            stunTimer = 0.1f;
        }

        if (currentHealth <= 0f)
        {
            // Knockback más largo al morir
            if (rb != null)
            {
                rb.linearVelocity = knockbackDir.normalized * knockbackForce * 0.5f;
                rb.linearDamping = hitDamping;
                stunTimer = 0.28f;
            }
            OnDeath?.Invoke();
            //Die();
        }
    }

    public float GetCurrentHealth() => currentHealth;

   
}

//private void Die()
//{
//    Debug.Log("¡Enemigo derrotado!");
//    Destroy(gameObject, 0.3f);
//}