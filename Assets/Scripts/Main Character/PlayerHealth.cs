using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 5f;
    public float currentHealth = 5f;

    [Header("Regeneration")]
    public bool enableRegeneration = false;
    public float regenerationRate = 2f;
    public float regenDelay = 3f;
    private bool regenActive = false;

    [Header("Invulnerability")]
    public float invulnerableTime = 1f;
    private bool invulnerable = false;

    private Coroutine regenRoutine;

    [Header("UI")]
    public HealthUI healthUI;
    public HealCounterUI healCounterUI;

    private PlayerController playerController;

    [Header("Debug/Testing")]
    public float healAmount = 1f;

    [Header("Curas (ya NO se reinician por escena)")]
    public int healsLeft; // se toma desde HealthDataNashe

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        if (healthUI != null)
            healthUI.Initialize(maxHealth);

        // Cargar curas desde el sistema persistente
        healsLeft = HealthDataNashe.Instance.healsLeft;
        UpdateHealCounterUI();

        if (regenRoutine != null)
            StopCoroutine(regenRoutine);
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
        if (HealthDataNashe.Instance.healsLeft <= 0)
        {
            Debug.Log("[Heal] No quedan curas.");
            return;
        }

        if (currentHealth <= 0f)
        {
            Debug.Log("[Heal] Estás muerto, no podés curarte.");
            return;
        }

        ModifyHealthFlat(healAmount);

        // Q restada
        HealthDataNashe.Instance.healsLeft--;
        healsLeft = HealthDataNashe.Instance.healsLeft;

        UpdateHealCounterUI();

        Debug.Log($"[Heal] Curación usada. Vida actual: {currentHealth}. Curas restantes: {healsLeft}");
    }

    public bool IsInvulnerable => invulnerable || (playerController != null && playerController.isInvulnerable);

    public void TakeDamage(float amount, Vector2 sourcePosition, float knockbackForce = 10f, float knockbackDuration = 0.2f)
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
        if (knockDir == Vector2.zero) knockDir = Vector2.up;

        var knockback = playerController.KnockbackState;
        knockback.SetKnockback(knockDir, knockbackForce, knockbackDuration);
        playerController.stateMachine.ChangeState(knockback);

        StartCoroutine(DamageFlash());
        StartCoroutine(TemporaryInvulnerability());

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
        if (!enableRegeneration) return;

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

    public void UpdateHealCounterUI()
    {
        if (healCounterUI != null)
            healCounterUI.SetHealsRemaining(
                HealthDataNashe.Instance.healsLeft,
                HealthDataNashe.Instance.maxHeals
            );
    }

    private void Die()
    {
        Debug.Log("Player Died");
        SceneManager.LoadScene("Lose");
    }
}
