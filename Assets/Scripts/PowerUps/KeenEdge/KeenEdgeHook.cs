using UnityEngine;
using System.Collections;

public class KeenEdgeHook : MonoBehaviour
{
    private EnemyHealth health;
    private float lastHealth;

    // Configuraci?n del sangrado
    private float bleedChance;
    private int bleedStartDamage;
    private float bleedCooldown;

    // Estado del sangrado / cooldown
    private bool isBleeding = false;
    private float nextBleedAllowedTime = 0f;

    // Para saber qui?n peg? (hitbox del player)
    private Collider2D lastHitSource;

    // VFX
    private GameObject bleedVfxPrefab;
    private GameObject activeBleedVfx;

    /// <summary>
    /// Inicializa el hook con los datos del power up.
    /// Llamado desde KeenEdgeObserver cuando se agrega al enemigo.
    /// </summary>
    public void Initialize(float chance, int startDamage, GameObject vfxPrefab, float cooldown)
    {
        bleedChance = chance;
        bleedStartDamage = startDamage;
        bleedVfxPrefab = vfxPrefab;
        bleedCooldown = cooldown;

        health = GetComponent<EnemyHealth>();
        if (health != null)
        {
            lastHealth = health.GetCurrentHealth();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Asumo que los colliders de ataque del player tienen este tag
        if (other.CompareTag("PlayerAttack"))
        {
            lastHitSource = other;
        }
    }

    private void Update()
    {
        if (health == null)
        {
            // Por si el EnemyHealth se asigna despu?s
            health = GetComponent<EnemyHealth>();
            if (health == null) return;

            lastHealth = health.GetCurrentHealth();
            return;
        }

        float currentHealth = health.GetCurrentHealth();

        // Si est? sangrando o en cooldown, no se intenta reaplicar
        if (isBleeding || Time.time < nextBleedAllowedTime)
        {
            lastHealth = currentHealth;
            return;
        }

        // Detectar da?o: baj? la vida y tenemos un hit registrado
        if (currentHealth < lastHealth && lastHitSource != null)
        {
            if (Random.value < bleedChance)
            {
                StartCoroutine(ApplyBleed());
                Debug.Log($"[KeenEdge] Sangrado aplicado a {name}");
            }

            lastHitSource = null;
        }

        lastHealth = currentHealth;
    }

    private IEnumerator ApplyBleed()
    {
        isBleeding = true;

        // Instanciar VFX una sola vez y parentear al enemigo
        if (bleedVfxPrefab != null && activeBleedVfx == null)
        {
            activeBleedVfx = Object.Instantiate(
                bleedVfxPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );
        }

        int damage = bleedStartDamage;

        while (damage > 0)
        {
            if (health == null || health.GetCurrentHealth() <= 0f)
            {
                break;
            }

            health.TakeDamage(damage, transform.position, 0f);
            Debug.Log($"[KeenEdge] {name} pierde {damage} por sangrado");
            damage--;

            yield return new WaitForSeconds(1f);
        }

        // Arranca el cooldown cuando termina el sangrado
        nextBleedAllowedTime = Time.time + bleedCooldown;

        // Apago / destruyo VFX
        if (activeBleedVfx != null)
        {
            Object.Destroy(activeBleedVfx);
            activeBleedVfx = null;
        }

        isBleeding = false;
    }

    private void OnDestroy()
    {
        // Limpieza de seguridad del VFX
        if (activeBleedVfx != null)
        {
            Object.Destroy(activeBleedVfx);
            activeBleedVfx = null;
        }
    }
}
