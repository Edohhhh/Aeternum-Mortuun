using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public int damage = 1;
    public float lifeTime = 0.1f;
    public Vector2 knockbackDir;
    public float knockbackForce = 100f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
               enemy.TakeDamage(damage, knockbackDir, knockbackForce);
            }
        }
    }
}
