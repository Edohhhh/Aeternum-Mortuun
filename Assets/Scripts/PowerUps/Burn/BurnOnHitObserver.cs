using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class BurnOnHitObserver : MonoBehaviour
{
    [Header("Burn")]
    public int damagePerSecond = 5;
    public float duration = 5f;
    public float cooldownPerEnemy = 10f;

    [Header("VFX")]
    public GameObject burnVfxPrefab;

    // Estado de quemadura por enemigo
    private Dictionary<GameObject, Coroutine> activeBurns = new();
    private Dictionary<GameObject, float> cooldowns = new();
    private Dictionary<GameObject, GameObject> activeVfx = new();

    [Header("Auto Hook")]
    [Tooltip("Cada cuántos segundos se escanean enemigos nuevos para agregarles EnemyBurnHook.")]
    public float rescanInterval = 0.5f;
    private float rescanTimer = 0f;

    public static BurnOnHitObserver Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Auto-scan para enemigos nuevos (oleadas, spawns, pooling, etc.)
        rescanTimer -= Time.deltaTime;
        if (rescanTimer <= 0f)
        {
            rescanTimer = rescanInterval;
            AttachHooksToExistingEnemies();
        }
    }

    /// <summary>
    /// Escanea todos los enemigos actuales y les agrega EnemyBurnHook si hace falta.
    /// </summary>
    public void AttachHooksToExistingEnemies()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            if (enemy.GetComponent<EnemyHealth>() != null &&
                enemy.GetComponent<EnemyBurnHook>() == null)
            {
                enemy.AddComponent<EnemyBurnHook>();
            }
        }
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
                Quaternion.identity,
                enemy.transform
            );

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
