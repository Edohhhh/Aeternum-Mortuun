using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    public int damage = 1;
    public float tick = 0.25f;
    private float nextTick;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time < nextTick) return;

        nextTick = Time.time + tick;

        var h = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
        if (h != null) h.TakeDamage(damage, other.transform.position);
    }
}