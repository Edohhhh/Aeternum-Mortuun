using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DashMark : MonoBehaviour
{
    private float shieldDuration = 3f;
    private float lifetime = 3f;

    public void Initialize(float shieldDuration, float lifetime)
    {
        this.shieldDuration = shieldDuration;
        this.lifetime = lifetime;
    }

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        // Aplicar escudo modular
        var shield = pc.GetComponent<ShieldGate>();
        if (shield == null) shield = pc.gameObject.AddComponent<ShieldGate>();
        shield.Activate(shieldDuration);

        Destroy(gameObject);
    }
}
