using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] public float damage = 1.5f;

    [Header("Opciones de daño")]
    [SerializeField] private bool useCollisionDamage = true;

    // --- Golpe manual para enemigos con animación (esqueleto, etc.) ---
    [Header("Golpe manual")]
    [SerializeField] private Transform attackPoint;   // asigná un hijo delante de la mano
    [SerializeField] private float hitRadius = 0.8f;  // radio visible en Scene
    [SerializeField] private LayerMask playerMask;    // capa del Player

    // Slime: daño por colisión física (queda tal cual)
    private void OnCollisionEnter2D(Collision2D c)
    {
        if (!useCollisionDamage) return;
        var h = c.collider.GetComponent<PlayerHealth>();
        if (h != null) h.TakeDamage(damage, transform.position);
    }

    // Esqueleto y otros: llamado desde el Animation Event
    public void DoDamageManual()
    {
        Vector2 origin = attackPoint ? (Vector2)attackPoint.position : (Vector2)transform.position;
        var hit = Physics2D.OverlapCircle(origin, hitRadius, playerMask);
        if (hit == null) return;

        var h = hit.GetComponent<PlayerHealth>() ?? hit.GetComponentInParent<PlayerHealth>();
        if (h != null) h.TakeDamage(damage, origin);


    }

    private void OnDrawGizmosSelected()
    {
        if (!attackPoint) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, hitRadius);
    }
}
//public class EnemyAttack : MonoBehaviour
//{
//    [SerializeField]public float damage = 1.5f;

//    private void OnCollisionEnter2D(Collision2D collision)
//    {
//        var health = collision.collider.GetComponent<PlayerHealth>();
//        if (health != null)
//        {
//            health.TakeDamage((float)damage, transform.position);
//        }
//    }
//}
