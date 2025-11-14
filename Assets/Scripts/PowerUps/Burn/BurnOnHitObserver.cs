using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BurnOnHitObserver : MonoBehaviour
{
    [Header("Burn")]
    public int damagePerSecond = 5;
    public float duration = 5f;
    public float cooldownPerEnemy = 10f;

    [Header("VFX")]
    public GameObject burnVfxPrefab;

    private Dictionary<GameObject, Coroutine> activeBurns = new();
    private Dictionary<GameObject, float> cooldowns = new();
    private Dictionary<GameObject, GameObject> activeVfx = new();

    public static BurnOnHitObserver Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ApplyBurn(GameObject enemy)
    {
        if (enemy == null) return;

        // Cooldown entre quemaduras
        if (cooldowns.TryGetValue(enemy, out float lastTime))
        {
            if (Time.time - lastTime < cooldownPerEnemy)
                return;
        }

        // Ya está quemándose
        if (activeBurns.ContainsKey(enemy)) return;

        // Activar estado de quemadura en EnemyBurnHook
        var hook = enemy.GetComponent<EnemyBurnHook>();
        if (hook != null) hook.SetBurning(true);

        // Iniciar rutina
        Coroutine routine = StartCoroutine(BurnRoutine(enemy));
        activeBurns[enemy] = routine;
        cooldowns[enemy] = Time.time;

        // Instanciar partícula
        if (burnVfxPrefab != null && !activeVfx.ContainsKey(enemy))
        {
            GameObject vfx = Instantiate(
                burnVfxPrefab,
                enemy.transform.position,
                Quaternion.identity
            );

            vfx.transform.SetParent(enemy.transform);
            vfx.transform.localPosition = Vector3.zero;

            activeVfx[enemy] = vfx;
        }
    }

    private IEnumerator BurnRoutine(GameObject enemy)
    {
        float elapsed = 0f;

        var health = enemy.GetComponent<EnemyHealth>();
        var hook = enemy.GetComponent<EnemyBurnHook>();

        while (elapsed < duration && enemy != null && health != null && health.GetCurrentHealth() > 0)
        {
            health.TakeDamage(damagePerSecond, enemy.transform.position, 0f);
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }

        // Termina quemadura
        if (hook != null)
            hook.SetBurning(false);

        activeBurns.Remove(enemy);

        // Destruir partícula
        if (enemy != null && activeVfx.TryGetValue(enemy, out GameObject vfx) && vfx != null)
            Destroy(vfx);

        activeVfx.Remove(enemy);
    }
}
