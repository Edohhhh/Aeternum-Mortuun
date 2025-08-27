using UnityEngine;

public class CursedBullet : MonoBehaviour
{
    [Header("Velocidad y daño")]
    public float speed = 5f;
    [Tooltip("Daño infligido al enemigo")]
    public int damage = 1;

    [Header("Configuración de disparo")]
    public float angleOffset = 0f; // Ángulo de desvío para múltiples proyectiles

    [Header("Efectos visuales")]
    public bool useVisualEffects = true;
    public Color bulletColor = Color.magenta;
    public float bulletSize = 1f;

    private Transform target;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Configurar efectos visuales
        if (useVisualEffects)
        {
            SetupVisualEffects();
        }

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
            Debug.LogWarning("🔍 CursedBullet: No se encontró enemigo cercano, destruyendo.");
            Destroy(gameObject);
            return;
        }

        // Aplicar dirección con offset
        ApplyDirectionWithOffset();
    }

    private void SetupVisualEffects()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = bulletColor;
            transform.localScale = Vector3.one * bulletSize;
        }

        // Añadir un pequeño efecto de rotación para que se vea más dinámico
        StartCoroutine(BulletRotationEffect());
    }

    private System.Collections.IEnumerator BulletRotationEffect()
    {
        float rotationSpeed = 360f; // Grados por segundo
        while (gameObject != null)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void ApplyDirectionWithOffset()
    {
        if (Mathf.Abs(angleOffset) > 0.1f)
        {
            Vector2 directionToTarget = (target.position - transform.position).normalized;
            float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            angle += angleOffset;
            Vector2 newDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            // Rotar el proyectil para apuntar en la nueva dirección
            float rotationAngle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
        }
        else
        {
            // Apuntar directamente al objetivo
            Vector2 directionToTarget = (target.position - transform.position).normalized;
            float rotationAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
        }
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Mover en la dirección a la que apunta
        transform.position += transform.right * speed * Time.deltaTime;

#if UNITY_EDITOR
        Debug.DrawLine(transform.position, target.position, Color.magenta);
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
                float knockbackForce = 5f;
                enemy.TakeDamage(damage, knockbackDir, knockbackForce);

                // Efecto visual al impactar
                CreateImpactEffect();
            }

            Destroy(gameObject);
        }
    }

    private void CreateImpactEffect()
    {
        // Pequeño efecto de impacto (opcional)
        if (useVisualEffects)
        {
            // Podrías instanciar un prefab de efecto de impacto aquí
            Debug.Log($"[CURSED BULLET] Impacto con daño: {damage}");
        }
    }
}