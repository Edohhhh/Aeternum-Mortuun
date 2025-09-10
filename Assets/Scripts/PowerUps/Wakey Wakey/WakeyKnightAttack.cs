using UnityEngine;

public class WakeyKnightAttack : MonoBehaviour
{
    public float damage = 5f;

    private void Start()
    {
        CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.4f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        var enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage((int)damage, Vector2.zero, 0f);
            Debug.Log($"[WakeyWakey] Atacó a {enemy.name} por {damage}");
        }
    }
}
