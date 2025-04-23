using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    public int damage = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                Vector2 sourcePosition = transform.parent.position; // posición del jugador
                enemy.TakeDamage(damage, sourcePosition);
            }
        }
    }
}