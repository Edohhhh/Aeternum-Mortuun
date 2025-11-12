using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Reflection;

public class BeggarValueObserver : MonoBehaviour
{
    [HideInInspector] public float bonusPerStat = 2f;

    // Lista de perks a las que "potenciamos"
    private readonly List<PowerUp> targetPerks = new();

    public void SetTargets(List<PowerUp> list)
    {
        targetPerks.Clear();
        if (list != null) targetPerks.AddRange(list);
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        ReapplyNow();
    }

    public void ReapplyNow()
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        var tracker = player.GetComponent<StatAugmentTracker>();
        if (tracker == null) tracker = player.gameObject.AddComponent<StatAugmentTracker>();

        tracker.RemoveFrom(player);

        var data = GameDataManager.Instance?.playerData;
        if (data == null) return;

        int addDamage = 0;
        float addMoveSpeed = 0f;
        float addMaxHealth = 0f;
        float multMoveSpeed = 1f; // multiplicador acumulado

        foreach (var perk in targetPerks)
        {
            if (perk == null) continue;
            bool hasIt = data.initialPowerUps != null && data.initialPowerUps.Contains(perk);
            if (!hasIt) continue;

            var t = perk.GetType();

            // --- SUMA DE STATS PLANOS ---
            if (HasPositiveField(t, perk, new[] { "flatDamage", "damageBonus", "extraDamage" }))
                addDamage += Mathf.RoundToInt(bonusPerStat);

            if (HasPositiveField(t, perk, new[] { "moveSpeedDelta", "speedBonus" }))
                addMoveSpeed += bonusPerStat;

            if (HasPositiveField(t, perk, new[] { "extraMaxHealth", "maxHealthIncrease" }))
                addMaxHealth += bonusPerStat;

            // --- MULTIPLICADORES (como speedMultiplier, damageMultiplier) ---
            if (TryGetMultiplierField(t, perk, "speedMultiplier", out float sm))
                multMoveSpeed *= (1f + (bonusPerStat * 0.01f)); // Ejemplo: +2 → +2%
        }

        // Aplicar efectos detectados
        tracker.ApplyTo(player, addDamage, addMoveSpeed, addMaxHealth, multMoveSpeed);
    }

    private bool HasPositiveField(System.Type type, object instance, string[] names)
    {
        foreach (var n in names)
        {
            var f = type.GetField(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f == null) continue;

            if (f.FieldType == typeof(int))
            {
                int v = (int)f.GetValue(instance);
                if (v > 0) return true;
            }
            else if (f.FieldType == typeof(float))
            {
                float v = (float)f.GetValue(instance);
                if (v > 0f) return true;
            }
        }
        return false;
    }

    private bool TryGetMultiplierField(System.Type type, object instance, string fieldName, out float value)
    {
        value = 0f;
        var f = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (f == null) return false;
        if (f.FieldType != typeof(float)) return false;

        value = (float)f.GetValue(instance);
        return value > 1f; // multiplicadores mayores a 1 = bonificación
    }
}
