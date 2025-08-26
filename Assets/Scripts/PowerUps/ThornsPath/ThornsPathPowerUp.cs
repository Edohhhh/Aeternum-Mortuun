using UnityEngine;

[CreateAssetMenu(fileName = "ThornsPathPowerUp", menuName = "PowerUps/Thorns Path")]
public class ThornsPathPowerUp : PowerUp
{
    [Header("Configuración del ThornsPath")]
    public GameObject thornsPathPrefab;
    public float activationProbability = 0.1f; // 10%
    public float thornsPathDuration = 4f;
    public float damagePerSecond = 10f;

    private ThornsPathObserver observerInstance;

    public override void Apply(PlayerController player)
    {
        if (observerInstance != null) return;

        observerInstance = new GameObject("ThornsPathObserver").AddComponent<ThornsPathObserver>();
        observerInstance.thornsPathPrefab = thornsPathPrefab;
        observerInstance.activationProbability = activationProbability;
        observerInstance.thornsPathDuration = thornsPathDuration;
        observerInstance.damagePerSecond = damagePerSecond;
        Object.DontDestroyOnLoad(observerInstance.gameObject);
    }

    public override void Remove(PlayerController player)
    {
        if (observerInstance != null)
        {
            Object.Destroy(observerInstance.gameObject);
            observerInstance = null;
        }
    }
}