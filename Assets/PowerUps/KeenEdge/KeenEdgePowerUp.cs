using UnityEngine;

[CreateAssetMenu(fileName = "KeenEdgePowerUp", menuName = "PowerUps/Keen Edge (Bleed)")]
public class KeenEdgePowerUp : PowerUp
{
    [Range(0f, 1f)] public float bleedChance = 0.05f;
    public int startingBleedDamage = 3;

    private GameObject instance;

    public override void Apply(PlayerController player)
    {
        if (instance != null) return;

        instance = new GameObject("KeenEdgeObserver");
        var observer = instance.AddComponent<KeenEdgeObserver>();
        observer.bleedChance = bleedChance;
        observer.bleedStartDamage = startingBleedDamage;

        Object.DontDestroyOnLoad(instance);
    }

    public override void Remove(PlayerController player)
    {
        if (instance != null)
        {
            Destroy(instance);
            instance = null;
        }
    }
}
