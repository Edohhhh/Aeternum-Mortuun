using UnityEngine;

public class EnemyBurnHook : MonoBehaviour
{
    private EnemyHealth health;
    private float lastHealth;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        if (health != null)
            lastHealth = health.GetCurrentHealth();
    }

    private void Update()
    {
        if (health == null) return;

        float current = health.GetCurrentHealth();
        if (current < lastHealth)
        {
            lastHealth = current;
            BurnOnHitObserver.Instance?.ApplyBurn(gameObject);
        }
        else if (current > lastHealth)
        {
            lastHealth = current;
        }
    }
}


