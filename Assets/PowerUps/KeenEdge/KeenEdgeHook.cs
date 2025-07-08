using UnityEngine;
using System.Collections;

public class KeenEdgeHook : MonoBehaviour
{
    private EnemyHealth health;
    private float lastHealth;

    private float bleedChance;
    private int bleedStartDamage;

    private bool isBleeding = false;
    private Collider2D lastHitSource;

    public void Initialize(float chance, int damage)
    {
        bleedChance = chance;
        bleedStartDamage = damage;

        health = GetComponent<EnemyHealth>();
        if (health != null)
            lastHealth = health.GetCurrentHealth();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Solo registrar si viene de una fuente de daño con tag PlayerAttack
        if (other.CompareTag("PlayerAttack"))
        {
            lastHitSource = other;
        }
    }

    private void Update()
    {
        if (health == null || isBleeding) return;

        float currentHealth = health.GetCurrentHealth();

        if (currentHealth < lastHealth && lastHitSource != null)
        {
            if (Random.value < bleedChance)
            {
                StartCoroutine(ApplyBleed());
                Debug.Log($"?? Desangrado aplicado a {name}");
            }

            lastHitSource = null;
        }

        lastHealth = currentHealth;
    }

    private IEnumerator ApplyBleed()
    {
        isBleeding = true;

        int damage = bleedStartDamage;
        while (damage > 0)
        {
            if (health == null || health.GetCurrentHealth() <= 0f) break;

            health.TakeDamage(damage, transform.position, 0f);
            Debug.Log($"?? Sangrado: {name} pierde {damage}");
            damage--;
            yield return new WaitForSeconds(1f);
        }

        isBleeding = false;
    }
}
