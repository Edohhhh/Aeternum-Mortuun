using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;

public static class GamblerEffects
{
    private static readonly List<CoroutineHost> hosts = new();

    public static void CleanupSceneTemp()
    {
        // cancelar coroutines temporales
        for (int i = 0; i < hosts.Count; i++)
            if (hosts[i] != null) hosts[i].StopAllAndDestroy();
        hosts.Clear();

        // limpiar buffs de daño del player (revierten en OnDestroy)
        var buffs = Object.FindObjectsOfType<PlayerTempDamage>(true);
        for (int i = 0; i < buffs.Length; i++)
            if (buffs[i] != null) Object.Destroy(buffs[i]);
    }

    private static CoroutineHost GetHost(GameObject target)
    {
        var host = target.GetComponent<CoroutineHost>();
        if (host == null) host = target.AddComponent<CoroutineHost>();
        hosts.Add(host);
        return host;
    }

    // ========== EFECTOS ==========

    public static void DamagePlayer(PlayerController player, int amount)
    {
        var ph = player.GetComponent<PlayerHealth>();
        if (ph == null) return;

        ph.currentHealth = Mathf.Max(0f, ph.currentHealth - amount);
        if (ph.healthUI != null)
            ph.healthUI.UpdateHearts(ph.currentHealth);

        Debug.Log($" [GamblingKnight] El jugador recibió {amount} de daño.");
    }

    public static void DamageAllEnemies(int amount)
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        int hits = 0;

        foreach (var e in enemies)
        {
            if (e == null) continue;
            var eh = e.GetComponent<EnemyHealth>();
            if (eh != null)
            {
                eh.TakeDamage(amount, Vector2.zero, 0f);
                hits++;
            }
        }

        if (hits > 0)
            Debug.Log($" [GamblingKnight] Se infligió {amount} de daño a {hits} enemigo(s).");
    }

    public static void FreezeAllEnemies(float seconds)
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        int frozen = 0;

        foreach (var e in enemies)
        {
            if (e == null) continue;
            if (e.GetComponent<FreezeEffect>() == null)
            {
                e.AddComponent<FreezeEffect>().Initialize(seconds);
                frozen++;
            }
        }

        if (frozen > 0)
            Debug.Log($"🎰 [GamblingKnight] {frozen} enemigo(s) congelados por {seconds:0.0}s.");
    }

    public static void BoostEnemiesSpeed(float delta, float seconds)
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        int boosted = 0;

        foreach (var e in enemies)
        {
            if (e == null) continue;
            var host = GetHost(e);
            host.Run(ApplySpeedDeltaViaReflection(e, delta, seconds));
            boosted++;
        }

        if (boosted > 0)
            Debug.Log($"🎰 [GamblingKnight] Aumentó la velocidad de {boosted} enemigo(s) en +{delta} por {seconds:0.0}s.");
    }

    public static void BoostPlayerSpeed(PlayerController player, float delta, float seconds)
    {
        var host = GetHost(player.gameObject);
        host.Run(PlayerSpeedBuff(player, delta, seconds));
        Debug.Log($"🎰 [GamblingKnight] ¡Velocidad del jugador +{delta} por {seconds:0.0}s!");
    }

    public static void BoostPlayerDamage(PlayerController player, int delta, float seconds)
    {
        if (player == null) return;
        var buff = player.gameObject.AddComponent<PlayerTempDamage>();
        buff.delta = delta;
        buff.duration = seconds;
        Debug.Log($"🎰 [GamblingKnight] ¡Daño del jugador +{delta} por {seconds:0.0}s!");
    }

    // ========== COROUTINES AUXILIARES ==========

    private static IEnumerator ApplySpeedDeltaViaReflection(GameObject enemy, float delta, float seconds)
    {
        if (enemy == null) yield break;

        var control = enemy.GetComponent<MonoBehaviour>(); // primer MB del enemigo
        if (control == null) yield break;

        var f = control.GetType().GetField("maxSpeed", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (f == null || f.FieldType != typeof(float)) yield break;

        float original = (float)f.GetValue(control);
        f.SetValue(control, original + delta);

        float t = 0f;
        while (t < seconds && enemy != null)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (enemy != null && f != null)
            f.SetValue(control, original);
    }

    private static IEnumerator PlayerSpeedBuff(PlayerController pc, float delta, float seconds)
    {
        if (pc == null) yield break;

        pc.moveSpeed += delta;

        float t = 0f;
        while (t < seconds && pc != null)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (pc != null)
            pc.moveSpeed -= delta;
    }
}

public class CoroutineHost : MonoBehaviour
{
    private readonly List<Coroutine> running = new();

    public void Run(IEnumerator routine)
    {
        var c = StartCoroutine(routine);
        running.Add(c);
    }

    public void StopAllAndDestroy()
    {
        for (int i = 0; i < running.Count; i++)
            if (running[i] != null) StopCoroutine(running[i]);
        running.Clear();
        Destroy(this);
    }
}
