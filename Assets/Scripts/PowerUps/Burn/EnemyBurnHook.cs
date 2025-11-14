using UnityEngine;
using System.Collections;

public class EnemyBurnHook : MonoBehaviour
{
    private EnemyHealth health;
    private float lastHealth;

    private bool waitingForNextBurn = false;
    private bool isBurning = false;

    public void SetBurning(bool value)
    {
        isBurning = value;
        // Opcional: cuando termina la quemadura, alinear la vida
        if (!value && health != null)
        {
            lastHealth = health.GetCurrentHealth();
        }
    }

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

        // Cualquier daño (sea por golpe o por burn)
        if (current < lastHealth)
        {
            // Solo disparamos nueva quemadura si:
            // - no está en cooldown
            // - no está quemándose ya
            if (!waitingForNextBurn && !isBurning)
            {
                BurnOnHitObserver.Instance.ApplyBurn(gameObject);
                StartCoroutine(BurnCooldown());
            }

            // SIEMPRE actualizamos el último valor de vida,
            // para que no vuelva a disparar con daño "viejo"
            lastHealth = current;
        }
        else if (current > lastHealth) // curación
        {
            lastHealth = current;
        }
    }

    private IEnumerator BurnCooldown()
    {
        waitingForNextBurn = true;
        yield return new WaitForSeconds(BurnOnHitObserver.Instance.cooldownPerEnemy);
        waitingForNextBurn = false;
    }
}
