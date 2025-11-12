using UnityEngine;


public class BloodRitualHook : MonoBehaviour
{
    private EnemyHealth enemyHealth;
    private PlayerController player;

    private float lastHealth;
    private float recentPlayerHitTimer = 0f;

    private float healChance;
    private float healAmount;

    // Ventana para correlacionar "baja de vida" con "hit del jugador"
    private const float detectionWindow = 0.35f;

    // Cache: si el player está null (cambio de escena), tratamos de re-enlazar
    private float tryRebindTimer = 0f;

    public void Initialize(EnemyHealth enemyHealth, float chance, float amount)
    {
        this.enemyHealth = enemyHealth;
        this.healChance = Mathf.Clamp01(chance);
        this.healAmount = Mathf.Max(0f, amount);
        this.lastHealth = SafeCurrentHealth();
    }

    public void SetPlayerRef(PlayerController pc)
    {
        this.player = pc;
    }

    private void Update()
    {
        if (enemyHealth == null) return;

        // Re-intento de enlace al player si se perdió por cambio de escena
        if (player == null)
        {
            tryRebindTimer -= Time.deltaTime;
            if (tryRebindTimer <= 0f)
            {
                player = Object.FindFirstObjectByType<PlayerController>();
                tryRebindTimer = 0.5f;
            }
        }

        if (recentPlayerHitTimer > 0f)
            recentPlayerHitTimer -= Time.deltaTime;

        float current = SafeCurrentHealth();

        // Si bajó la vida desde el último frame y la baja ocurrió dentro de la ventana posterior a un "hit" del player
        if (current < lastHealth && recentPlayerHitTimer > 0f)
        {
            TryHealPlayer();
            recentPlayerHitTimer = 0f; // consume la ventana
        }

        lastHealth = current;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Aceptamos por TAG o por COMPONENTE para robustez
        if (other.CompareTag("PlayerAttack") || other.GetComponent<AttackHitbox>() != null)
        {
            recentPlayerHitTimer = detectionWindow;
        }
    }

    private void TryHealPlayer()
    {
        if (player == null) return;
        if (Random.value >= healChance) return;

        var ph = player.GetComponent<PlayerHealth>();
        if (ph == null) return;

        float before = ph.currentHealth;
        ph.currentHealth = Mathf.Min(ph.currentHealth + healAmount, ph.maxHealth);

        // UI
        if (ph.healthUI != null)
        {
            ph.healthUI.Initialize(ph.maxHealth); // por si cambia a medios corazones
            ph.healthUI.UpdateHearts(ph.currentHealth);
        }

        Debug.Log($"🩸 [BloodRitual] Curación aplicada al jugador: +{healAmount} ({before} → {ph.currentHealth}).");
    }

    private float SafeCurrentHealth()
    {

        try
        {
            
            var mi = enemyHealth.GetType().GetMethod("GetCurrentHealth");
            if (mi != null) return (float)mi.Invoke(enemyHealth, null);
        }
        catch { /* ignorar */ }

       
        var fi = enemyHealth.GetType().GetField("currentHealth");
        if (fi != null && fi.FieldType == typeof(float))
            return (float)fi.GetValue(enemyHealth);

        
        var fi2 = enemyHealth.GetType().GetField("health");
        if (fi2 != null)
        {
            if (fi2.FieldType == typeof(float)) return (float)fi2.GetValue(enemyHealth);
            if (fi2.FieldType == typeof(int)) return (int)fi2.GetValue(enemyHealth);
        }

       
        return 0f;
    }
}
