using UnityEngine;

public class ContactDamager : MonoBehaviour
{
    [SerializeField] private float damage = 1f;
    [SerializeField] private float tick = 0.25f;
    [SerializeField] private LayerMask playerMask;

    private Collider2D col;
    private float nextTick;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;

        // SIEMPRE apagado al inicio:
        enabled = false;
        if (col) col.enabled = false;
    }

    public void SetEnabled(bool v)
    {
        enabled = v;
        if (!col) col = GetComponent<Collider2D>();
        if (col) col.enabled = v;
        nextTick = 0f; // reset del tick
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!enabled) return;
        if (((1 << other.gameObject.layer) & playerMask) == 0) return;
        if (Time.time < nextTick) return;

        nextTick = Time.time + tick;

        var h = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
        if (h != null) h.TakeDamage(damage, transform.position);
    }
}