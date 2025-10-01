using System.Linq;
using UnityEngine;

public class SpectralBullet : MonoBehaviour
{
    [Header("Velocidad y daño")]
    public float speed = 5f;
    [Tooltip("Daño infligido al enemigo")]
    public int damage = 1;

    [Header("Rotación")]
    [Tooltip("Si el sprite de la bala apunta hacia la derecha en su art (vector local +X), dejá true. Si apunta hacia arriba (+Y), marcá false.")]
    public bool spriteFacesRight = true;

    [Tooltip("Si > 0, la rotación será suave; si 0 la rotación será instantánea.")]
    public float rotationSpeed = 0f; // segundos para completar la rotación (si 0 => instantánea)

    private Transform target;

    void Start()
    {
        FindClosestEnemy();

        if (target == null)
        {
            Debug.LogWarning("🔍 SpectralBullet: No se encontró enemigo cercano, destruyendo.");
            Destroy(gameObject);
        }
        else
        {
            // Opcional: rotar inmediatamente al target al lanzarse
            RotateToTargetInstant();
        }
    }

    private void FindClosestEnemy()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (var e in enemies)
        {
            if (e == null) continue;
            float dist = Vector2.Distance(transform.position, e.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = e.transform;
            }
        }

        target = closest;
    }

    void Update()
    {
        // si el objetivo murió entre frames, intentar retargetear a otro enemigo cercano
        if (target == null)
        {
            FindClosestEnemy();
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }
        }

        // 1) Rotación: mirar hacia el objetivo (instante o suave)
        Vector3 dir = (target.position - transform.position).normalized;

        // Calculamos el ángulo deseado según el eje del sprite
        float targetAngleDeg;
        if (spriteFacesRight)
        {
            // sprite apunta a la derecha -> vector local +X
            targetAngleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }
        else
        {
            // sprite apunta hacia arriba -> vector local +Y
            targetAngleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        }

        Quaternion targetRot = Quaternion.Euler(0f, 0f, targetAngleDeg);

        if (rotationSpeed <= 0f)
        {
            transform.rotation = targetRot; // rotación instantánea
        }
        else
        {
            // Smooth rotation: usamos Lerp con Time.deltaTime / rotationSpeed
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 360f * Time.deltaTime / Mathf.Max(0.0001f, rotationSpeed));
        }

        // 2) Movimiento hacia el objetivo (manteniendo la orientación)
        transform.position += (Vector3)dir * speed * Time.deltaTime;

#if UNITY_EDITOR
        Debug.DrawLine(transform.position, target.position, Color.cyan);
#endif
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy"))
            return;

        var enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            // knockback en función de la posición del enemigo respecto de la bala (consistente)
            Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
            float knockbackForce = 5f; // podés ajustar

            enemy.TakeDamage(damage, knockbackDir, knockbackForce);
        }

        Destroy(gameObject);
    }

    // Rotación instantánea útil al instanciar
    private void RotateToTargetInstant()
    {
        if (target == null) return;
        Vector3 dir = (target.position - transform.position).normalized;

        float angle;
        if (spriteFacesRight)
            angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        else
            angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}

