using UnityEngine;

[CreateAssetMenu(fileName = "BombPowerUp", menuName = "PowerUps/Bomb")]
public class BombPowerUp : PowerUp
{
    [Header("Prefabs y configuración")]
    public GameObject bombPrefab;

    [Range(0f, 1f)]
    [Tooltip("Probabilidad (entre 0 y 1) de invocar una bomba al completar un combo.")]
    public float spawnChance = 0.1f;

    public override void Apply(PlayerController player)
    {
        var existing = GameObject.Find("BombObserver");
        BombComboObserver obs;

        if (existing == null)
        {
            var go = new GameObject("BombObserver");
            go.name = "BombObserver";
            obs = go.AddComponent<BombComboObserver>();
            Object.DontDestroyOnLoad(go);
        }
        else
        {
            obs = existing.GetComponent<BombComboObserver>();
        }

        // Configurar/actualizar bindings
        obs.player = player;               // se reasigna también al cambiar de escena
        obs.bombPrefab = bombPrefab;
        obs.spawnChance = spawnChance;
    }

    public override void Remove(PlayerController player)
    {
        var observer = GameObject.Find("BombObserver");
        if (observer != null)
            Object.Destroy(observer);
    }
}
