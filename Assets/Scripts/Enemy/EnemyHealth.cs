using UnityEngine;
using System.Collections;

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

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private readonly Color damageColor = Color.red;
    private float flashDuration = 0.1f;


    private void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearDamping = baseDamping;
            rb.gravityScale = 1f;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

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

  
        if (spriteRenderer != null)
        {
            StopCoroutine(FlashRed());
            StartCoroutine(FlashRed());
        }


        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            Vector2 knockbackVelocity = knockbackDir.normalized * knockbackForce;
            rb.linearVelocity = knockbackVelocity;
            rb.linearDamping = hitDamping;
            stunTimer = 0.1f;
        }

        if (currentHealth <= 0f)
        {
            if (rb != null)
            {
                rb.linearVelocity = knockbackDir.normalized * knockbackForce * 0.5f;
                rb.linearDamping = hitDamping;
                stunTimer = 0.28f;
            }
            OnDeath?.Invoke();
        }
    }

    private IEnumerator FlashRed()
    {
        float half = flashDuration * 0.5f;
        float t = 0f;
        // Fade in a rojo
        while (t < half)
        {
            spriteRenderer.color = Color.Lerp(originalColor, damageColor, t / half);
            t += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = damageColor;

        t = 0f;
        while (t < half)
        {
            spriteRenderer.color = Color.Lerp(damageColor, originalColor, t / half);
            t += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = originalColor;
    }
    public float GetCurrentHealth() => currentHealth;
}


//private void Die()
//{
//    Debug.Log("¡Enemigo derrotado!");
//    Destroy(gameObject, 0.3f);
//}