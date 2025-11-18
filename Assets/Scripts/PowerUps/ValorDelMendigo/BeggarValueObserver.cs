using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class BeggarValueObserver : MonoBehaviour
{
    [HideInInspector] public float bonusPerStat = 2f;

    // Lista de perks a las que "potenciamos" leyendo de la lista
    private readonly List<PowerUp> targetPerks = new();

    // 👉 Cuántas veces el jugador ha usado LifeUp
    private int lifeUpStacks = 0;

    // 👉 Flag: solo cuando es true aplicamos el bonus de vida en esta llamada
    private bool applyLifeBonusThisCall = false;

    public void SetTargets(List<PowerUp> list)
    {
        targetPerks.Clear();
        if (list != null) targetPerks.AddRange(list);
    }

    private void Awake()
    {
        // Solo persistimos. NO usamos SceneManager acá.
        DontDestroyOnLoad(gameObject);
    }

    // 🔹 Helper estático genérico (AttackUp, SpeedUp, ruletas, etc.)
    public static void RequestReapply()
    {
        var go = GameObject.Find("BeggarValueObserver");
        if (go == null) return;

        var obs = go.GetComponent<BeggarValueObserver>();
        if (obs != null)
            obs.ReapplyNow();
    }

    // 🔹 Helper ESPECIAL para LifeUp: suma un stack y marca que esta vez sí hay bonus de vida
    public static void NotifyLifeUpApplied()
    {
        var go = GameObject.Find("BeggarValueObserver");
        if (go == null) return;

        var obs = go.GetComponent<BeggarValueObserver>();
        if (obs != null)
        {
            obs.lifeUpStacks++;               // llevamos conteo total de LifeUps usados
            obs.applyLifeBonusThisCall = true; // esta llamada SÍ debe tocar vida
            obs.ReapplyNow();
        }
    }

    /// <summary>
    /// Recalcula desde cero el bonus del Mendigo según las perks actuales
    /// (AttackUp/SpeedUp) y, solo cuando corresponde, el bonus de vida por LifeUp.
    /// </summary>
    public void ReapplyNow()
    {
        var player = Object.FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        // Asegurar tracker
        var tracker = player.GetComponent<StatAugmentTracker>();
        if (tracker == null)
            tracker = player.gameObject.AddComponent<StatAugmentTracker>();

        // Quitar cualquier buff previo del mendigo
        tracker.RemoveFrom(player);

        // ==== Perks actuales (para daño/velocidad) ====
        List<PowerUp> perks = null;
        if (player.initialPowerUps != null && player.initialPowerUps.Length > 0)
        {
            perks = new List<PowerUp>(player.initialPowerUps);
        }
        else
        {
            var data = GameDataManager.Instance?.playerData;
            if (data != null && data.initialPowerUps != null)
                perks = data.initialPowerUps;
        }

        if (perks == null)
            perks = new List<PowerUp>();

        int addDamage = 0;
        float addMoveSpeed = 0f;
        float addMaxHealth = 0f;
        float multMoveSpeed = 1f;

        foreach (var perkAsset in targetPerks)
        {
            if (perkAsset == null) continue;

            int copies = 0;
            foreach (var p in perks)
            {
                if (p == perkAsset)
                    copies++;
            }
            if (copies <= 0) continue;

            if (perkAsset is AttackUp)
            {
                addDamage += Mathf.RoundToInt(bonusPerStat * copies);
            }

            if (perkAsset is PlayerSpeedPowerUp)
            {
                addMoveSpeed += bonusPerStat * copies;
            }
        }

        if (applyLifeBonusThisCall && lifeUpStacks > 0)
        {
            addMaxHealth += bonusPerStat * lifeUpStacks;
        }

        tracker.ApplyTo(player, addDamage, addMoveSpeed, addMaxHealth, multMoveSpeed);

        applyLifeBonusThisCall = false;
    }
}
