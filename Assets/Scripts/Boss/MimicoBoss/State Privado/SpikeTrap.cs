using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SpikeTrap : MonoBehaviour
{
    [Header("Da�o por tick (igual que AcidTrail)")]
    [SerializeField] private float damagePerSecond = 1f;
    [SerializeField] private float damageInterval = 1f;   // cada cu�nto aplica da�o

    private float timer = 0f;
    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;          // igual que el �cido, usamos trigger
    }

    private void OnEnable()
    {
        timer = 0f;                    // por si se usa desde pool
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // igual que AcidTrail: acumulamos tiempo y da�amos por intervalos
        var health = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
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
            timer = 0f;                // reset al salir, igual que AcidTrail
        }
    }
}