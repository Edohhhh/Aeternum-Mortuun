using UnityEngine;

[CreateAssetMenu(fileName = "KeenEdgePowerUp", menuName = "PowerUps/Keen Edge (Bleed)")]
public class KeenEdgePowerUp : PowerUp
{
    [Header("Bleed Settings")]
    [Range(0f, 1f)] public float bleedChance = 0.05f;
    public int startingBleedDamage = 3;
    public float bleedCooldown = 2f;

    [Header("VFX")]
    public GameObject bleedVfxPrefab;

    private GameObject observerInstance;

    public override void Apply(PlayerController player)
    {
        if (observerInstance != null) return;

        observerInstance = new GameObject("KeenEdgeObserver");
        var observer = observerInstance.AddComponent<KeenEdgeObserver>();

        observer.bleedChance = bleedChance;
        observer.bleedStartDamage = startingBleedDamage;
        observer.bleedCooldown = bleedCooldown;
        observer.bleedVfxPrefab = bleedVfxPrefab;

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
