using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "DarkSacrificePowerUp", menuName = "PowerUps/Dark Sacrifice")]
public class DarkSacrificePowerUp : PowerUp
{
    [Header("Prefab de la marca")]
    public GameObject markerPrefab;

    [Header("Spawn")]
    [Tooltip("Cada cuántos segundos aparece una nueva marca bajo el jugador")]
    public float spawnIntervalSeconds = 6f;

    [Tooltip("Cuánto dura la marca antes de desaparecer")]
    public float markerLifetimeSeconds = 3f;

    [Tooltip("Pequeño ajuste vertical (negativo = un poco más abajo)")]
    public float yOffset = 0f;

    public override void Apply(PlayerController player)
    {
        if (markerPrefab == null)
        {
            Debug.LogWarning("[DarkSacrifice] Prefab no asignado.");
            return;
        }

        // Singleton del observer
        var existing = GameObject.Find("DarkSacrificeObserver");
        DarkSacrificeObserver obs;
        if (existing == null)
        {
            var go = new GameObject("DarkSacrificeObserver");
            obs = go.AddComponent<DarkSacrificeObserver>();
            go.name = "DarkSacrificeObserver";
            Object.DontDestroyOnLoad(go);
        }
        else
        {
            obs = existing.GetComponent<DarkSacrificeObserver>();
        }

        obs.markerPrefab = markerPrefab;
        obs.spawnInterval = Mathf.Max(0.5f, spawnIntervalSeconds);
        obs.markerLifetime = Mathf.Max(0.2f, markerLifetimeSeconds);
        obs.yOffset = yOffset;

        // Vincular player actual (el observer se re-vincula solo en cada escena)
        obs.BindPlayer(player);
        obs.EnableSpawning(true);
    }

    public override void Remove(PlayerController player)
    {
        var go = GameObject.Find("DarkSacrificeObserver");
        if (go != null) Object.Destroy(go);
    }
}
