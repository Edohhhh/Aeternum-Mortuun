using UnityEngine;

[CreateAssetMenu(fileName = "SlowOnHitPowerUp", menuName = "PowerUps/Slow On Any Damage")]
public class SlowOnHitPowerUp : PowerUp
{
    public float slowPercent = 0.05f;
    public float Slowduration = 2f;

    public override void Apply(PlayerController player)
    {
        // Crear el observer
        if (Object.FindAnyObjectByType<GlobalEnemyDamageObserver>() == null)
        {
            var go = new GameObject("GlobalEnemyDamageObserver");
            var observer = go.AddComponent<GlobalEnemyDamageObserver>();
            observer.slowPercent = slowPercent;
            observer.duration = Slowduration;
            Object.DontDestroyOnLoad(go);
        }

        // Agregar hook a todos los enemigos activos
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (enemy.GetComponent<EnemyDamageHook>() == null && enemy.GetComponent<EnemyHealth>() != null)
            {
                enemy.AddComponent<EnemyDamageHook>();
            }
        }
    }

    public override void Remove(PlayerController player)
    {
        // Nada, efecto es temporal y autocontrolado
    }
}

