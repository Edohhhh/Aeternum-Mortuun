using UnityEngine;

public class EnemyBurnHook : MonoBehaviour
{
    private EnemyHealth health;
    private float lastHealth;
    private bool waitingForNextBurn = false;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        if (health != null)
            lastHealth = health.GetCurrentHealth();
    }

    private void Update()
    {
        if (health == null || BurnOnHitObserver.Instance == null) return;

        float current = health.GetCurrentHealth();

        // Detectar daño nuevo
        if (current < lastHealth && !waitingForNextBurn)
        {
            lastHealth = current;
            BurnOnHitObserver.Instance.ApplyBurn(gameObject);
            StartCoroutine(BurnCooldown());
        }
        else if (current > lastHealth)
        {
            lastHealth = current;
        }
    }

    private System.Collections.IEnumerator BurnCooldown()
    {
        waitingForNextBurn = true;
        yield return new WaitForSeconds(BurnOnHitObserver.Instance.cooldownPerEnemy);
        waitingForNextBurn = false;
    }
}