using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BerserkerObserver : MonoBehaviour
{
    private PlayerController player;
    private PlayerHealth health;
    private SpriteRenderer spriteRenderer;

    [Header("Configuración")]
    public float healthLossThreshold = 2f;
    public float activeDuration = 10f;
    public float bonusSpeed = 2f;
    public int bonusDamage = 5;
    public float bonusAttackSpeed = 0.5f;
    public Color berserkerColor = Color.red;

    private float lastHealth;
    private float accumulatedLoss = 0f;
    private bool isActive = false;
    private float berserkTimer;

    private float originalMoveSpeed;
    private float originalAttackCooldown;
    private Color originalColor;

    private readonly List<GameObject> trackedHitboxes = new();

    public void Initialize(PlayerController p)
    {
        player = p;
        health = p.GetComponent<PlayerHealth>();
        spriteRenderer = p.GetComponent<SpriteRenderer>();

        originalColor = spriteRenderer.color;
        lastHealth = health.currentHealth;

        originalMoveSpeed = player.moveSpeed;

        var combat = player.GetComponent<CombatSystem>();
        if (combat != null)
            originalAttackCooldown = combat.attackCooldown;

        StartCoroutine(CheckHealthLoop());
    }

    private IEnumerator CheckHealthLoop()
    {
        while (true)
        {
            float current = health.currentHealth;
            float delta = lastHealth - current;

            if (delta > 0f)
            {
                accumulatedLoss += delta;
                lastHealth = current;

                if (!isActive && accumulatedLoss >= healthLossThreshold)
                {
                    ActivateBerserker();
                    accumulatedLoss = 0f;
                }
            }

            if (isActive)
            {
                berserkTimer -= Time.deltaTime;
                if (berserkTimer <= 0f)
                    DeactivateBerserker();
            }

            DetectNewHitboxes();

            yield return null;
        }
    }

    private void ActivateBerserker()
    {
        isActive = true;
        berserkTimer = activeDuration;

        player.moveSpeed += bonusSpeed;

        var combat = player.GetComponent<CombatSystem>();
        if (combat != null)
            combat.attackCooldown -= bonusAttackSpeed;

        if (spriteRenderer != null)
            spriteRenderer.color = berserkerColor;

        Debug.Log("🩸 BERSERKER ACTIVADO");
    }

    private void DeactivateBerserker()
    {
        isActive = false;
        player.moveSpeed = originalMoveSpeed;

        var combat = player.GetComponent<CombatSystem>();
        if (combat != null)
            combat.attackCooldown = originalAttackCooldown;

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        trackedHitboxes.Clear();
        Debug.Log("💤 BERSERKER DESACTIVADO");
    }

    private void DetectNewHitboxes()
    {
        var hitboxes = player.GetComponentsInChildren<AttackHitbox>(true);
        foreach (var hitbox in hitboxes)
        {
            if (!trackedHitboxes.Contains(hitbox.gameObject))
            {
                if (isActive)
                {
                    hitbox.damage += bonusDamage;
                    Debug.Log($"[BERSERKER] +{bonusDamage} daño a hitbox: {hitbox.name}");
                }

                trackedHitboxes.Add(hitbox.gameObject);
            }
        }
    }
}
