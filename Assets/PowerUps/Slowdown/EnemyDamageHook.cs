using UnityEngine;

public class EnemyDamageHook : MonoBehaviour
{
    private EnemyHealth health;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.OnDamaged += HandleDamaged;
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDamaged -= HandleDamaged;
        }
    }

    private void HandleDamaged()
    {
        GlobalEnemyDamageObserver.RegisterDamage(gameObject);
    }
}
