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

            
            health.TakeDamage(damage, other.transform.position);

            
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
