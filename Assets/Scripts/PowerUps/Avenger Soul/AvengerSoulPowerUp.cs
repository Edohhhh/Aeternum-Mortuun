using UnityEngine;

[CreateAssetMenu(fileName = "AvengerSoulPowerUp", menuName = "PowerUps/Avenger Soul")]
public class AvengerSoulPowerUp : PowerUp
{
    public GameObject[] spiritPrefabs;
    public float delayAfterHit = 1f;

    private GameObject observerInstance;

    public override void Apply(PlayerController player)
    {
        if (observerInstance != null) return;

        observerInstance = new GameObject("AvengerSoulObserver");
        var observer = observerInstance.AddComponent<AvengerSoulObserver>();
        observer.spiritPrefabs = spiritPrefabs;
        observer.delayAfterHit = delayAfterHit;

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
