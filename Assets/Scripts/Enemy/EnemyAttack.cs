using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float damage = 1.5f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var health = collision.collider.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage((int)damage, transform.position);
        }
    }
}
