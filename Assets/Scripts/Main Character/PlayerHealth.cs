using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 5f;
    public float currentHealth = 5f;

    [Header("Regeneration")]
    [Tooltip("Si está activo, el jugador regenerará vida después de recibir/esperar regenDelay.")]
    public bool enableRegeneration = false;     // <-- Nuevo toggle
    public float regenerationRate = 2f;
    public float regenDelay = 3f;
    private bool regenActive = false;

    [Header("Invulnerability")]
    public float invulnerableTime = 1f;
    private bool invulnerable = false;

    public bool IsInvulnerable => invulnerable || (playerController != null && playerController.isInvulnerable);

    private Coroutine regenRoutine;

    [Header("UI")]
    public HealthUI healthUI;
    public HealCounterUI healCounterUI;

    private PlayerController playerController;

    [Header("Debug/Testing")]
    public float healAmount = 1f;

    [Header("Heal (Q) - Límite por escena")]
    [Tooltip("Cuántas veces puede curarse el jugador por escena con Q.")]
    public int maxHealsPerScene = 3;
    private int healsLeft;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        if (healthUI != null) healthUI.Initialize(maxHealth);

        healsLeft = maxHealsPerScene;
        UpdateHealCounterUI();

        // Asegurar que no haya coroutines de regen colgando
        if (regenRoutine != null) StopCoroutine(regenRoutine);
        regenActive = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryUseHeal();
        }
    }

    private void TryUseHeal()
    {
        if (healsLeft <= 0)
        {
            UnityEngine.Debug.Log("[Heal] No quedan curas (Q) para esta escena.");
            return;
        }

        if (currentHealth <= 0f)
        {
            UnityEngine.Debug.Log("[Heal] No puedes curarte: estás muerto.");
            return;
        }

        ModifyHealthFlat(healAmount);

        healsLeft--;
        UpdateHealCounterUI();

        UnityEngine.Debug.Log($"[Heal] Usada Q. Curó {healAmount}. Health actual: {currentHealth}. Q restantes: {healsLeft}");

        // Nota: NO reiniciamos el delay de regen aquí para evitar que usar Q active la regeneración.
        // Si preferís lo contrario, descomenta la siguiente línea:
        // RestartRegenDelay();
    }

    public void TakeDamage(float amount, Vector2 sourcePosition)
    {
        if (IsInvulnerable || (playerController != null && playerController.stateMachine.CurrentState == playerController.KnockbackState))
            return;

        currentHealth -= amount;
        UpdateUI();

        if (currentHealth <= 0f)
        {
            Die();
            return;
        }

        Vector2 knockDir = (transform.position - (Vector3)sourcePosition).normalized;
        var knockback = playerController.KnockbackState;
        knockback.SetKnockback(knockDir, 10f, 0.2f);
        playerController.stateMachine.ChangeState(knockback);

        StartCoroutine(DamageFlash());
        StartCoroutine(TemporaryInvulnerability());

        // Reinicia la cuenta atrás de regeneración SOLO si la regeneración está habilitada
        if (enableRegeneration)
            RestartRegenDelay();
    }

    public void ModifyHealthFlat(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateUI();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void ModifyHealthPercent(float percent)
    {
        float delta = maxHealth * percent;
        ModifyHealthFlat(delta);
    }

    private IEnumerator TemporaryInvulnerability()
    {
        invulnerable = true;
        yield return new WaitForSeconds(invulnerableTime);
        invulnerable = false;
    }

    private void RestartRegenDelay()
    {
        if (!enableRegeneration) return; // protección extra

        if (regenRoutine != null)
            StopCoroutine(regenRoutine);
        regenRoutine = StartCoroutine(RegenRoutine());
    }

    private IEnumerator RegenRoutine()
    {
        yield return new WaitForSeconds(regenDelay);
        regenActive = true;
        while (regenActive && currentHealth < maxHealth)
        {
            ModifyHealthFlat(regenerationRate * Time.deltaTime);
            yield return null;
        }
        regenActive = false;
    }

    private IEnumerator DamageFlash()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color originalColor = sr.color;
        sr.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        sr.color = originalColor;
    }

    public void UpdateUI()
    {
        if (healthUI != null)
            healthUI.UpdateHearts(currentHealth);
    }

    private void UpdateHealCounterUI()
    {
        if (healCounterUI != null)
            healCounterUI.SetHealsRemaining(healsLeft, maxHealsPerScene);
    }

    private void Die()
    {
        UnityEngine.Debug.Log("Player Died");
        SceneManager.LoadScene("Lose");
    }

    private void OnEnable()
    {
        healsLeft = maxHealsPerScene;
        UpdateHealCounterUI();
    }
}
