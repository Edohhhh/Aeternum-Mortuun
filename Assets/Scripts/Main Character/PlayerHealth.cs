using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Diagnostics;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 5f;
    public float currentHealth = 5f;

    [Header("Regeneration")]
    public float regenerationRate = 2f;
    public float regenDelay = 3f;
    private bool regenActive = false;

    [Header("Invulnerability")]
    public float invulnerableTime = 1f;
    private bool invulnerable = false;

    public bool IsInvulnerable => invulnerable || playerController.isInvulnerable;

    private Coroutine regenRoutine;

    [Header("UI")]
    public HealthUI healthUI;
    private PlayerController playerController;

    [Header("Debug/Testing")]
    public float healAmount = 1f; // cuanto cura al apretar Q

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        if (healthUI != null)
            healthUI.Initialize(maxHealth);
    }

    private void Update()
    {
        // --- Atajo para curar con Q ---
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ModifyHealthFlat(healAmount);
            UnityEngine.Debug.Log($"[Heal] Player healed {healAmount}. Current health: {currentHealth}");
        }
    }

    public void TakeDamage(float amount, Vector2 sourcePosition)
    {
        UnityEngine.Debug.Log($"[TakeDamage] Llamado con amount: {amount} desde: {sourcePosition}");
        UnityEngine.Debug.Log(new StackTrace(1, true));

        if (IsInvulnerable || playerController.stateMachine.CurrentState == playerController.KnockbackState)
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

    private void Die()
    {
        SceneManager.LoadScene("Lose");
        UnityEngine.Debug.Log("Player Died");
    }
}
