using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 5f;    // represents number of hearts
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
    public HealthUI healthUI;       // assign in inspector

    private void Start()
    {
        currentHealth = maxHealth;
        if (healthUI != null)
            healthUI.Initialize(maxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (invulnerable) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateUI();

        StartCoroutine(InvulnerabilityRoutine());
        RestartRegenDelay();

        if (currentHealth <= 0f)
            Die();
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

    private IEnumerator InvulnerabilityRoutine()
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

    private void UpdateUI()
    {
        if (healthUI != null)
            healthUI.UpdateHearts(currentHealth);
    }

    private void Die()
    {
        Debug.Log("Player Died");
    }
}
