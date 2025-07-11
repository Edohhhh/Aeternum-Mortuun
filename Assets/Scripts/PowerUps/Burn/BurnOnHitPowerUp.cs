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

        Object.DontDestroyOnLoad(observerInstance);

        // Hookear todos los enemigos activos en la escena
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (enemy.GetComponent<EnemyHealth>() != null && enemy.GetComponent<EnemyBurnHook>() == null)
            {
                enemy.AddComponent<EnemyBurnHook>();

            }
        }
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
