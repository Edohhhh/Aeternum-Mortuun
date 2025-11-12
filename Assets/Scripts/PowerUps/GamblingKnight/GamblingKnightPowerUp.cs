using UnityEngine;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public enum SlotOutcomeType
{
    Nothing,
    DamagePlayer_1,
    DamageEnemies_10,
    DamagePlayer_1_Bis,     // duplicado opcional con otro peso
    FreezeEnemies_2s,
    SpeedEnemies_Plus5_3s,
    SpeedPlayer_Plus3_5s,
    PlayerDamage_Plus1_5s,  // +1 daño al jugador por 5s
    Jackpot_1000_AllEnemies // poné weight = 0 si “no sale nunca”
}

[Serializable]
public struct WeightedOutcome
{
    [Range(0f, 100f)] public float weight; // peso/chance relativa
    public SlotOutcomeType type;
    public Sprite iconOverHead;            // ícono que aparece sobre el player
}

[CreateAssetMenu(fileName = "GamblingKnightPowerUp", menuName = "PowerUps/GamblingKnight?!")]
public class GamblingKnightPowerUp : PowerUp
{
    [Header("Máquina")]
    public GameObject slotMachinePrefab;
    [Tooltip("Distancia para spawnear detrás del player")]
    public float spawnBehindDistance = 1.6f;

    [Header("Interacción")]
    [Tooltip("Cooldown entre tiradas")]
    public float rollCooldown = 10f;

    [Header("Resultados por pesos")]
    public List<WeightedOutcome> outcomes = new List<WeightedOutcome>();

    public override void Apply(PlayerController player)
    {
        var existing = GameObject.Find("GamblingKnightObserver");
        GamblingKnightObserver obs;
        if (existing == null)
        {
            var go = new GameObject("GamblingKnightObserver");
            obs = go.AddComponent<GamblingKnightObserver>();
            go.name = "GamblingKnightObserver";
            Object.DontDestroyOnLoad(go);
        }
        else
        {
            obs = existing.GetComponent<GamblingKnightObserver>();
        }

        obs.slotMachinePrefab = slotMachinePrefab;
        obs.spawnBehindDistance = spawnBehindDistance;
        obs.rollCooldown = rollCooldown;
        obs.outcomes = outcomes;

        obs.SpawnForCurrentScene(); // spawnea ya en la escena actual
    }

    public override void Remove(PlayerController player)
    {
        var go = GameObject.Find("GamblingKnightObserver");
        if (go != null) Object.Destroy(go);
    }
}
