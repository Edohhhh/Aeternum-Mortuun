using UnityEngine;

[CreateAssetMenu(fileName = "PestTrailPowerUp", menuName = "PowerUps/Pest Trail")]
public class PestTrailPowerUp : PowerUp
{
    public GameObject acidCloudPrefab;
    public float spawnInterval = 5f;

    private GameObject observerInstance;

    public override void Apply(PlayerController player)
    {
        if (observerInstance != null) return;

        observerInstance = new GameObject("PestTrailObserver");
        var observer = observerInstance.AddComponent<PestTrailObserver>();
        observer.Initialize(player.transform, acidCloudPrefab, spawnInterval);
        Object.DontDestroyOnLoad(observerInstance);
    }

    public override void Remove(PlayerController player)
    {
        if (observerInstance != null)
        {
            Object.Destroy(observerInstance);
            observerInstance = null;
        }
    }
}
