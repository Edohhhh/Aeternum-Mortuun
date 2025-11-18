using UnityEngine;

/// Daño por tick para pilares/raíles del Diablo (independiente de SpikeTrap)
[RequireComponent(typeof(Collider2D))]
public class DevilBeamDamage : MonoBehaviour
{
    [SerializeField] private float damagePerSecond = 1f;
    [SerializeField] private float damageInterval = 0.25f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private string requiredTag = "Player";

    private float timer;

    /// Configuración desde quien instancia
    public void Init(float dps, float interval, LayerMask mask, string tagFilter = "Player")
    {
        damagePerSecond = dps;
        damageInterval = interval;
        targetMask = mask;
        requiredTag = tagFilter;
    }

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnEnable() { timer = 0f; }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!PassesFilter(other)) return;

        var health = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
        if (!health) return;

        timer += Time.deltaTime;
        if (timer >= damageInterval)
        {
            timer = 0f;
            int dmg = Mathf.CeilToInt(damagePerSecond);
            health.TakeDamage(dmg, other.transform.position);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!PassesFilter(other)) return;
        timer = 0f;
    }

    private bool PassesFilter(Collider2D other)
    {
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return false;
        return (targetMask.value & (1 << other.gameObject.layer)) != 0;
    }
}
