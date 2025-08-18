using UnityEngine;

public class SpectralBullet : MonoBehaviour
{
    [Header("Velocidad y daño")]
    public float speed = 5f;
    [Tooltip("Daño infligido al enemigo")]
    public int damage = 1;

    private Transform target;

    void Start()
    {
        // Buscar enemigo más cercano
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (var e in enemies)
        {
            float dist = Vector2.Distance(transform.position, e.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = e.transform;
            }
        }

        target = closest;

        if (target == null)
        {
            Debug.LogWarning("🔍 SpectralBullet: No se encontró enemigo cercano, destruyendo.");
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Mover hacia el objetivo
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

#if UNITY_EDITOR
        Debug.DrawLine(transform.position, target.position, Color.cyan); // Línea de depuración
#endif
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
                float knockbackForce = 5f; // podés ajustar la fuerza como quieras
                enemy.TakeDamage(damage, knockbackDir, knockbackForce);

            }

            Destroy(gameObject);
        }
    }
}
