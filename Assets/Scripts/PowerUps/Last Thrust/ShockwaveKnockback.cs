using UnityEngine;

public class ShockwaveKnockback : MonoBehaviour
{
    public float radius = 2.5f;
    public float strength = 12f;
    public LayerMask enemyLayer;

    private void Start()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player")) continue; // nunca afectes al player

            GameObject enemy = hit.gameObject;

            if (!enemy.TryGetComponent(out ModularKnockbackReceiver receiver))
            {
                receiver = enemy.AddComponent<ModularKnockbackReceiver>();
            }

            Vector2 dir = (enemy.transform.position - transform.position).normalized;
            receiver.ApplyKnockback(dir, strength);
        }

        Destroy(gameObject, 0.25f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
