using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AcidCloud : MonoBehaviour
{
    public int damagePerTick = 1;
    public float tickInterval = 1f;
    public float lifetime = 3f;

    private HashSet<EnemyHealth> enemiesInZone = new();

    private void Start()
    {
        Destroy(gameObject, lifetime);
        StartCoroutine(DamageOverTime());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        var enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemiesInZone.Add(enemy);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        var enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemiesInZone.Remove(enemy);
        }
    }

    private IEnumerator DamageOverTime()
    {
        while (true)
        {
            foreach (var enemy in enemiesInZone)
            {
                if (enemy != null)
                    enemy.TakeDamage(damagePerTick, transform.position, 0f);
            }

            yield return new WaitForSeconds(tickInterval);
        }
    }
}