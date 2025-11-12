using UnityEngine;

public class FreezeEffect : MonoBehaviour
{
    private float duration;
    private float timer;
    private bool active;

    private MonoBehaviour[] comps;
    private Rigidbody2D rb;
    private Vector2 storedVel;
    private float storedDrag;

    public void Initialize(float seconds)
    {
        duration = seconds;
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            storedVel = rb.linearVelocity;
            storedDrag = rb.linearDamping;
            rb.linearVelocity = Vector2.zero;
            rb.linearDamping = 9999f;
        }

        comps = GetComponents<MonoBehaviour>();
        foreach (var c in comps) if (c != this && c.enabled) c.enabled = false;

        active = true;
        timer = 0f;
    }

    private void Update()
    {
        if (!active) return;
        timer += Time.deltaTime;
        if (timer >= duration) End();
    }

    private void End()
    {
        if (rb != null)
        {
            rb.linearVelocity = storedVel;
            rb.linearDamping = storedDrag;
        }
        foreach (var c in comps) if (c != null && c != this) c.enabled = true;
        Destroy(this);
    }
}
