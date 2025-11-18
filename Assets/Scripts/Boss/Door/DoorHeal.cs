using UnityEngine;

/// <summary>
/// CUMPLE 3 REQUISITOS (sin modificar EnemyHealth):
/// 1. Se suscribe al evento OnDamaged de EnemyHealth.
/// 2. Si el daño NO es letal, cura al enemigo a maxHealth.
/// 3. Si el daño ES letAL, permite que el enemigo muera y se destruya.
/// </summary>
[RequireComponent(typeof(EnemyHealth))]
public class HealOnHit : MonoBehaviour
{
    private EnemyHealth enemyHealth;
    private bool isDead = false;

    // Bandera para evitar un bucle infinito de curación
    // (TakeDamage -> OnDamaged -> Heal -> TakeDamage -> OnDamaged...)
    private bool isHealing = false;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();

        // 1. Suscribirse a AMBOS eventos
        enemyHealth.OnDamaged += HandleDamage; // Se llama ANTES de la comprobación de muerte
        enemyHealth.OnDeath += HandleDeath;   // Se llama DESPUÉS de la comprobación de muerte
    }

    private void OnDestroy()
    {
        // Desuscribirse para evitar errores
        if (enemyHealth != null)
        {
            enemyHealth.OnDamaged -= HandleDamage;
            enemyHealth.OnDeath -= HandleDeath;
        }
    }

    /// <summary>
    /// REQUISITO 1: Se llama CADA VEZ que TakeDamage() es invocado.
    /// Se ejecuta DESPUÉS de que currentHealth se reduce, pero ANTES
    /// de que EnemyHealth compruebe si la vida es <= 0.
    /// </summary>
    private void HandleDamage()
    {
        // Si ya estamos curando o muertos, no hacer nada.
        if (isHealing || isDead) return;

        // --- COMPROBACIÓN CRÍTICA ---
        // Comprobamos la vida actual DESPUÉS de que el daño fue aplicado.
        if (enemyHealth.GetCurrentHealth() <= 0f)
        {
            // REQUISITO 3: El daño FUE letal.
            // No hacemos nada y dejamos que EnemyHealth continúe
            // su ejecución para que llame a OnDeath.
            Debug.Log($"[{gameObject.name}] Recibió daño LETAL. No se curará.");
            return;
        }

        // --- SI LLEGAMOS AQUÍ, EL DAÑO NO FUE LETAL ---

        // REQUISITO 2: Curar al máximo
        float currentHealth = enemyHealth.GetCurrentHealth();
        float maxHealth = enemyHealth.maxHealth;

        // Solo curar si es necesario
        if (currentHealth < maxHealth)
        {
            float healthToRestore = maxHealth - currentHealth;
            int healAmount = Mathf.CeilToInt(healthToRestore);

            Debug.Log($"[{gameObject.name}] Recibió {maxHealth - currentHealth} de daño, curando {healAmount}.");

            // --- INICIO DEL TRUCO ANTI-BUCLE ---
            // 1. Levantamos la bandera.
            isHealing = true;

            // 2. Usamos el truco: daño negativo para curar.
            // Esto llamará a TakeDamage() OTRA VEZ, lo que disparará
            // OnDamaged OTRA VEZ.
            enemyHealth.TakeDamage(-healAmount, Vector2.zero, 0f);

            // 3. Cuando la línea 2 termine, HandleDamage() habrá sido
            // llamado de nuevo, pero habrá visto "isHealing = true"
            // y se habrá salido inmediatamente.
            // Ahora bajamos la bandera, listos para el *próximo* golpe.
            isHealing = false;
            // --- FIN DEL TRUCO ANTI-BUCLE ---
        }
    }

    /// <summary>
    /// REQUISITO 3 (Parte B): Esta función solo se llamará si
    /// HandleDamage() detectó daño letal y permitió que OnDeath se disparara.
    /// </summary>
    private void HandleDeath()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"[{gameObject.name}] Murió por daño letal. Destruyendo.");

        // Notificar al Manager
        EnemyManager.Instance?.UnregisterEnemy();

        // Destruir el objeto
        Destroy(gameObject, 0.3f);
    }
}