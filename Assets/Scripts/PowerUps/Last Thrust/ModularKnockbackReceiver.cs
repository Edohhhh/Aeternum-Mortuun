using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ModularKnockbackReceiver : MonoBehaviour
{
    public float decay = 18f;
    public float maxSpeed = 22f;

    private Vector2 knockVel;
    private float knockTimer;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void ApplyKnockback(Vector2 direction, float strength)
    {
        if (strength <= 0f) return;

        Vector2 impulse = direction.sqrMagnitude > 0.0001f ? direction.normalized * strength : Vector2.zero;
        knockVel += impulse;
        knockVel = Vector2.ClampMagnitude(knockVel, maxSpeed);
        knockTimer = 0.08f; // como en el player
    }

    private void FixedUpdate()
    {
        if (knockTimer > 0)
        {
            rb.MovePosition(rb.position + knockVel * Time.fixedDeltaTime);
            knockVel = Vector2.MoveTowards(knockVel, Vector2.zero, decay * Time.fixedDeltaTime);
            knockTimer -= Time.fixedDeltaTime;
        }
    }
}
