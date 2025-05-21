using UnityEngine;

public class SpikeHazard : MonoBehaviour
{
    public float damage = 50f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si el objeto tiene HealthSystem, le aplicamos daño
        HealthSystem health = other.GetComponent<HealthSystem>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }
    }
}
