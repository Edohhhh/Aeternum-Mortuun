using UnityEngine;

public class AcidCloud : MonoBehaviour
{
    public int damage = 5;
    public float lifetime = 3f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        var enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, Vector2.zero, 0f);
        }
    }
}
