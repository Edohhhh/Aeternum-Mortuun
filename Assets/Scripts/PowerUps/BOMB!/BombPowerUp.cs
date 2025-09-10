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
        if (GameObject.Find("BombObserver") != null) return;

        GameObject observer = new GameObject("BombObserver");
        var bombTrigger = observer.AddComponent<BombComboObserver>();
        bombTrigger.player = player;
        bombTrigger.bombPrefab = bombPrefab;
        bombTrigger.spawnChance = spawnChance;

        Object.DontDestroyOnLoad(observer);
    }

    public override void Remove(PlayerController player)
    {
        var observer = GameObject.Find("BombObserver");
        if (observer != null)
            Object.Destroy(observer);
    }
}
