using UnityEngine;

[CreateAssetMenu(fileName = "BurnOnHitPowerUp", menuName = "PowerUps/Burn On Enemy Hit")]
public class BurnOnHitPowerUp : PowerUp
{
    public int damagePerSecond = 5;
    public float burnDuration = 5f;
    public float cooldownPerEnemy = 10f;

    private GameObject observerInstance;

    public override void Apply(PlayerController player)
    {
        if (observerInstance != null) return;

        // Crear el observer global
        observerInstance = new GameObject("BurnOnHitObserver");
        var observer = observerInstance.AddComponent<BurnOnHitObserver>();
        observer.damagePerSecond = damagePerSecond;
        observer.duration = burnDuration;
        observer.cooldownPerEnemy = cooldownPerEnemy;

        // Agregar el auto-hook manager
        observerInstance.AddComponent<BurnHookManager>();

        Object.DontDestroyOnLoad(observerInstance);
    }

    public override void Remove(PlayerController player)
    {
        if (observerInstance != null)
        {
            Object.Destroy(observerInstance);
            observerInstance = null;
        }

        foreach (var extra in Object.FindObjectsByType<BurnOnHitObserver>(FindObjectsSortMode.None))
        {
            Object.Destroy(extra.gameObject);
        }

    }
}