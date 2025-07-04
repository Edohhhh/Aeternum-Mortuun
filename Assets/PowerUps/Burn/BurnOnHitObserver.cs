using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BurnOnHitObserver : MonoBehaviour
{
    public int damagePerSecond = 5;
    public float duration = 5f;
    public float cooldownPerEnemy = 10f;

    private Dictionary<GameObject, Coroutine> activeBurns = new();
    private Dictionary<GameObject, float> cooldowns = new();

    public static BurnOnHitObserver Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ApplyBurn(GameObject enemy)
    {
        if (enemy == null) return;

        if (cooldowns.TryGetValue(enemy, out float lastTime))
        {
            if (Time.time - lastTime < cooldownPerEnemy)
                return; // a�n en cooldown
        }

        if (!activeBurns.ContainsKey(enemy))
        {
            Coroutine routine = StartCoroutine(BurnRoutine(enemy));
            activeBurns[enemy] = routine;
            cooldowns[enemy] = Time.time;
        }
    }

    private IEnumerator BurnRoutine(GameObject enemy)
    {
        float elapsed = 0f;

        while (elapsed < duration && enemy != null)
        {
            var health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
                health.TakeDamage(damagePerSecond, enemy.transform.position, 0f);

            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }

        activeBurns.Remove(enemy);
    }
}
