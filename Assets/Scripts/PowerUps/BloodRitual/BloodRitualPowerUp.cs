using UnityEngine;

[CreateAssetMenu(fileName = "BloodRitualPowerUp", menuName = "PowerUps/Blood Ritual")]
public class BloodRitualPowerUp : PowerUp
{
    [Range(0f, 1f)] public float healChance = 0.1f;
    public float healAmount = 0.5f;

    private GameObject observerInstance;

    public override void Apply(PlayerController player)
    {
        if (observerInstance != null) return;

        observerInstance = new GameObject("BloodRitualObserver");
        var observer = observerInstance.AddComponent<BloodRitualObserver>();
        observer.healChance = healChance;
        observer.healAmount = healAmount;
        observer.player = player;

        Object.DontDestroyOnLoad(observerInstance);
    }

    public override void Remove(PlayerController player)
    {
        if (observerInstance != null)
        {
            GameObject.Destroy(observerInstance);
            observerInstance = null;
        }
    }
}

