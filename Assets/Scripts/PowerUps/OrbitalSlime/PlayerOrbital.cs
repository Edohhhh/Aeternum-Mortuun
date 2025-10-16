using UnityEngine;

public class PlayerOrbital : MonoBehaviour
{
    private Transform target;
    private float orbitRadius;
    private float rotationSpeed;
    private int damagePerSecond;
    private float angle;
    private float damageTimer;

    public void Initialize(Transform target, float radius, float speed, int dps)
    {
        this.target = target;
        orbitRadius = radius;
        rotationSpeed = speed;
        damagePerSecond = dps;
    }

    public void SetInitialAngle(float startAngle)
    {
        angle = startAngle;
    }

    void Update()
    {
        if (target == null) return;

        angle += rotationSpeed * Time.deltaTime;
        float radians = angle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * orbitRadius;
        transform.position = target.position + offset;

        damageTimer += Time.deltaTime;
        if (damageTimer >= 1f)
        {
            damageTimer = 0f;
            DamageEnemies();
        }
    }

    private void DamageEnemies()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.3f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                var enemy = hit.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                    enemy.TakeDamage(damagePerSecond, knockbackDir, 0f);
                }
            }
        }
    }
}
