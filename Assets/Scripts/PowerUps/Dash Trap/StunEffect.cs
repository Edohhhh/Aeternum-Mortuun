using UnityEngine;

public class StunEffect : MonoBehaviour
{
    private float duration;
    private float timer;

    private bool isStunned = false;
    private MonoBehaviour[] enemyBehaviours;

    private Rigidbody2D rb;
    private Vector2 storedVelocity;
    private float originalLinearDrag;

    public void Initialize(float stunDuration)
    {
        duration = stunDuration;
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            storedVelocity = rb.linearVelocity;
            rb.linearVelocity = Vector2.zero;

            originalLinearDrag = rb.linearDamping;
            rb.linearDamping = 9999f; // frena
        }

        enemyBehaviours = GetComponents<MonoBehaviour>();
        foreach (var comp in enemyBehaviours)
        {
            if (comp != this && comp.enabled)
                comp.enabled = false;
        }

        isStunned = true;
        timer = 0f;
    }

    private void Update()
    {
        if (!isStunned) return;

        timer += Time.deltaTime;
        if (timer >= duration)
        {
            EndStun();
        }
    }

    private void EndStun()
    {
        if (rb != null)
        {
            rb.linearDamping = originalLinearDrag;
            rb.linearVelocity = storedVelocity;
        }

        foreach (var comp in enemyBehaviours)
        {
            if (comp != this && comp != null)
                comp.enabled = true;
        }

        Destroy(this);
    }
}