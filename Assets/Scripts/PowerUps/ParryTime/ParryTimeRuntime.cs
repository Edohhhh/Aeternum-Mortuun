using UnityEngine;
using System;
using System.Collections;
using System.Reflection;

[DisallowMultipleComponent]
public class ParryTimeRuntime : MonoBehaviour
{
    private GameObject effectPrefab;
    private float parryWindow;
    private float invulnDur;
    private int damageAll;
    private float btScale;
    private float btDur;
    private float parryCd;

    private PlayerHealth health;

    private bool windowOpen = false;
    private bool invuln = false;
    private bool cdLock = false;

    private bool noEventFallback = false;
    private float lastHealth;

    private Delegate cachedHandler;

    public void Setup(GameObject fx, float window, float invuln, int damageAllOnParry,
                      float bulletScale, float bulletDur, float cooldown)
    {
        effectPrefab = fx;
        parryWindow = window;
        invulnDur = invuln;
        damageAll = damageAllOnParry;
        btScale = bulletScale;
        btDur = bulletDur;
        parryCd = cooldown;
    }

    private void Start()
    {
        health = GetComponent<PlayerHealth>();

        if (health != null)
        {
            if (!TrySubscribeOnDamagedInt())
            {
                noEventFallback = true;
                lastHealth = health.currentHealth;
            }
        }
        else
        {
            noEventFallback = true;
        }
    }

    private void OnDestroy()
    {
        TryUnsubscribeOnDamagedInt();
    }

    // Suscripción directa a OnDamaged(int) si existe
    private bool TrySubscribeOnDamagedInt()
    {
        try
        {
            var evt = health.GetType().GetEvent("OnDamaged");
            if (evt == null) return false;

            var invoke = evt.EventHandlerType.GetMethod("Invoke");
            var pars = invoke.GetParameters();
            if (pars.Length == 1 && pars[0].ParameterType == typeof(int))
            {
                Action<int> handler = OnDamaged_Int;
                cachedHandler = Delegate.CreateDelegate(evt.EventHandlerType, handler.Target, handler.Method);
                evt.AddEventHandler(health, cachedHandler);
                return true;
            }
        }
        catch { /* ignoramos y usamos fallback */ }

        return false;
    }

    private void TryUnsubscribeOnDamagedInt()
    {
        try
        {
            if (health == null || cachedHandler == null) return;
            var evt = health.GetType().GetEvent("OnDamaged");
            if (evt != null)
            {
                evt.RemoveEventHandler(health, cachedHandler);
            }
        }
        catch { /* ignorar */ }
    }

    private void OnDamaged_Int(int amount)
    {
        if (health == null) return;

        if (!invuln)
        {
            if (windowOpen)
            {
                // Revertir el daño del frame
                health.currentHealth += amount;
                if (health.healthUI != null) health.healthUI.UpdateHearts(health.currentHealth);
                TriggerParry();
            }
        }
        else
        {
            // Durante invuln, anular daños
            health.currentHealth += amount;
            if (health.healthUI != null) health.healthUI.UpdateHearts(health.currentHealth);
        }
    }

    private void Update()
    {
        // Abrir ventana con Espacio
        if (Input.GetKeyDown(KeyCode.Space) && !cdLock)
        {
            StartCoroutine(OpenParryWindow());
        }

        // Fallback: delta de vida si no hay evento
        if (noEventFallback && health != null)
        {
            if (!invuln && health.currentHealth < lastHealth)
            {
                int lost = Mathf.RoundToInt(lastHealth - health.currentHealth);
                if (windowOpen)
                {
                    health.currentHealth += lost;
                    if (health.healthUI != null) health.healthUI.UpdateHearts(health.currentHealth);
                    TriggerParry();
                }
            }
            else if (invuln && health.currentHealth < lastHealth)
            {
                int lost = Mathf.RoundToInt(lastHealth - health.currentHealth);
                health.currentHealth += lost;
                if (health.healthUI != null) health.healthUI.UpdateHearts(health.currentHealth);
            }

            lastHealth = health.currentHealth;
        }
    }

    private IEnumerator OpenParryWindow()
    {
        windowOpen = true;
        cdLock = true;
        yield return new WaitForSeconds(parryWindow);
        windowOpen = false;
        yield return new WaitForSeconds(parryCd);
        cdLock = false;
    }

    private void TriggerParry()
    {
        // FX
        if (effectPrefab != null)
            Instantiate(effectPrefab, transform.position, Quaternion.identity);

        // Invulnerabilidad
        StartCoroutine(InvulnerabilityFor(invulnDur));

        // Daño a todos los enemigos
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            var e = enemies[i];
            if (e == null) continue;
            var eh = e.GetComponent<EnemyHealth>();
            if (eh != null) eh.TakeDamage(damageAll, Vector2.zero, 0f);
        }

        // Bullet time
        StartCoroutine(BulletTime(btScale, btDur));
    }

    private IEnumerator InvulnerabilityFor(float seconds)
    {
        invuln = true;
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime; // NO se ve afectado por bullet time
            yield return null;
        }
        invuln = false;
    }

    private IEnumerator BulletTime(float scale, float duration)
    {
        float original = Time.timeScale;
        float originalFixed = Time.fixedDeltaTime;

        Time.timeScale = Mathf.Clamp(scale, 0.01f, 1f);
        Time.fixedDeltaTime = originalFixed * Time.timeScale;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // tiempo real
            yield return null;
        }

        Time.timeScale = original;
        Time.fixedDeltaTime = originalFixed;
    }
}
