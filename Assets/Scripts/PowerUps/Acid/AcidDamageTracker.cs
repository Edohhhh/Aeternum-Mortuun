using UnityEngine;
using System.Collections.Generic;

public static class AcidDamageTracker
{
    private static Dictionary<EnemyHealth, float> lastDamageTimes = new();

    public static bool CanDamage(EnemyHealth enemy, float interval)
    {
        float currentTime = Time.time;

        if (!lastDamageTimes.ContainsKey(enemy) || currentTime - lastDamageTimes[enemy] >= interval)
        {
            lastDamageTimes[enemy] = currentTime;
            return true;
        }

        return false;
    }

    public static void Clear(EnemyHealth enemy)
    {
        if (lastDamageTimes.ContainsKey(enemy))
            lastDamageTimes.Remove(enemy);
    }
}