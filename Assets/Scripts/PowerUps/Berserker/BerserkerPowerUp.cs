using UnityEngine;

[CreateAssetMenu(fileName = "BerserkerPowerUp", menuName = "PowerUps/Berserker")]
public class BerserkerPowerUp : PowerUp
{
    private GameObject observerGO;

    public override void Apply(PlayerController player)
    {
        if (observerGO != null) return;

        observerGO = new GameObject("BerserkerObserver");
        var observer = observerGO.AddComponent<BerserkerObserver>();
        observer.Initialize(player);
        Object.DontDestroyOnLoad(observerGO);
    }

    public override void Remove(PlayerController player)
    {
        if (observerGO != null)
        {
            Object.Destroy(observerGO);
            observerGO = null;
        }
    }
}
