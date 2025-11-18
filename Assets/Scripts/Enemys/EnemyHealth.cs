using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    public GameObject impactEffectPrefab;
    public bool IsStunned => stunTimer > 0f;

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

    [Header("Partículas al recibir daño")]
    [SerializeField] private GameObject hitParticlePrefab;

    [Header("FX Daño (números)")]
    [SerializeField] private DamageNumberFX damageNumberPrefab;


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


    public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
    {
        currentHealth -= damage;

        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        }
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

        if (hitParticlePrefab != null)
        {
            var fx = Instantiate(hitParticlePrefab, transform.position, Quaternion.identity);
            var bones = fx.GetComponent<BoneBurstFX>();
            if (bones != null) bones.Init(knockbackDir);
        }

        if (damageNumberPrefab != null)
        {
            // Un pequeño offset arriba del enemigo para que no se tape con el sprite
            Vector3 spawnPos = transform.position + new Vector3(0f, 0.4f, 0f);
            var num = Instantiate(damageNumberPrefab, spawnPos, Quaternion.identity);
            num.Init(damage, knockbackDir); // usamos la dirección del golpe que ya recibís aquí
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