using UnityEngine;

public class EnemyCritHook : MonoBehaviour
{
    private float lastHealth;
    private EnemyHealth health;
    private CriticalHitObserver observer;

    private float lastCritTime = -999f;
    private float critCooldown = 0.2f;

    public void Init(CriticalHitObserver obs, EnemyHealth hp)
    {
        observer = obs;
        health = hp;
        lastHealth = hp.GetCurrentHealth();
    }

    private void Update()
    {
        if (health == null || observer == null) return;

        float current = health.GetCurrentHealth();

        if (current < lastHealth)
        {
            if (Time.time - lastCritTime > critCooldown && WasHitByPlayer() && observer.ShouldApplyCrit())
            {
                lastCritTime = Time.time;
                health.TakeDamage(1, transform.position, 0f);
            }

            lastHealth = current;
        }
        else if (current > lastHealth)
        {
            lastHealth = current;
        }
    }

    private bool WasHitByPlayer()
    {
        return true;
    }
}
