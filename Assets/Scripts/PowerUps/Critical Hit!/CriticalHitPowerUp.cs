using UnityEngine;

[CreateAssetMenu(fileName = "CriticalHitPowerUp", menuName = "PowerUps/Critical Hit")]
public class CriticalHitPowerUp : PowerUp
{
    public float critChance = 0.05f;

    private GameObject observerInstance;
    private int originalBaseDamage; 

    public override void Apply(PlayerController player)
    {
        if (observerInstance != null) return;

        // Guardar baseDamage y duplicarlo
        originalBaseDamage = player.baseDamage;
        player.baseDamage = player.baseDamage * 2;

        observerInstance = new GameObject("CritObserver");
        var observer = observerInstance.AddComponent<CriticalHitObserver>();
        observer.critChance = critChance;

        Object.DontDestroyOnLoad(observerInstance);
    }

    public override void Remove(PlayerController player)
    {
        // Restaurar baseDamage original
        player.baseDamage = originalBaseDamage;

        if (observerInstance != null)
        {
            Destroy(observerInstance);
            observerInstance = null;
        }
    }
}
