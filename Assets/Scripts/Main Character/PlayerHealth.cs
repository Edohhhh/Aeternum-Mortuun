using UnityEngine;
using System.Collections;

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
    private Coroutine regenRoutine;

    [Header("UI")]
    public HealthUI healthUI;
    private PlayerController playerController;

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

    public void TakeDamage(int amount, Vector2 sourcePosition)
    {
        if (invulnerable || playerController.stateMachine.CurrentState == playerController.KnockbackState)
            return;

        currentHealth -= amount;
        UpdateUI();

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
        Debug.Log("Player Died");
    }
}
