using UnityEngine;

public class AcidTrail : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float damagePerSecond = 0.5f;
    [SerializeField] private float damageInterval = 1f;  // cada cuánto aplicar daño (ej. 1 segundo)

    private float timer = 0f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null) return;

        timer += Time.deltaTime;
        if (timer >= damageInterval)
        {
            timer = 0f;
            int damage = Mathf.CeilToInt(damagePerSecond);

            //  Mandamos la posición del jugador como "source" para anular el knockback
            health.TakeDamage(damage, other.transform.position);

            Debug.Log($"[ACID] Dañando al jugador con {damage} desde {other.transform.position} (sin knockback)");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            timer = 0f; // reinicia si sale del ácido
        }
    }
}
