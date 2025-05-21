using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public AttackData[] comboAttacks; // lista de ataques
    public Transform attackPoint;
    public LayerMask enemyLayers;

    private int comboIndex = 0;
    private float lastAttackTime;
    private float comboResetTime = 1.0f;

    void Update()
    {
        Vector2 mouseDir = GetMouseDirection();

        // Rotar el attackPoint hacia el mouse
        float angle = Mathf.Atan2(mouseDir.y, mouseDir.x) * Mathf.Rad2Deg;
        attackPoint.rotation = Quaternion.Euler(0f, 0f, angle);

        // Moverlo a una distancia fija desde el centro
        float distance = 1f;
        attackPoint.position = transform.position + (Vector3)(mouseDir.normalized * distance);

        // Flip visual opcional
        transform.localScale = new Vector3(mouseDir.x < 0 ? -1 : 1, 1, 1);

        if (Input.GetMouseButtonDown(0))
        {
            TryAttack(mouseDir);
        }

        if (Time.time - lastAttackTime > comboResetTime)
        {
            comboIndex = 0;
        }
    }

    Vector2 GetMouseDirection()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        return ((Vector2)(mouseWorld - transform.position)).normalized;
    }

    void TryAttack(Vector2 dir)
    {
        if (comboAttacks.Length == 0) return;

        var attack = comboAttacks[comboIndex];
        if (Time.time >= lastAttackTime + attack.cooldown)
        {
            PerformAttack(attack, dir);
            lastAttackTime = Time.time;

            // Avanzar combo
            comboIndex = (comboIndex + 1) % comboAttacks.Length;
        }
    }

    void PerformAttack(AttackData attack, Vector2 dir)
    {
        Vector2 attackPosition = (Vector2)attackPoint.position + dir.normalized * attack.range;

        // DEBUG: Ver posición de ataque
        Debug.Log("Punto de ataque: " + attackPosition + " | Radio: " + attack.hitboxRadius);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPosition, attack.hitboxRadius, enemyLayers);
        Debug.Log("Enemigos detectados: " + hitEnemies.Length);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy == null)
            {
                Debug.LogWarning("Collider detectado es nulo.");
                continue;
            }

            Debug.Log("Colisionado con: " + enemy.name);

            var health = enemy.GetComponent<HealthSystem>();
            if (health != null)
            {
                Debug.Log("Golpeando a: " + enemy.name);
                health.TakeDamage((int)attack.damage);
            }
            else
            {
                Debug.LogWarning("El objeto " + enemy.name + " no tiene HealthSystem");
            }
        }

        // Animación si tenés Animator
        // anim.SetTrigger(attack.attackName);
        Debug.Log("Performed attack: " + attack.attackName);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Vector2 mouseDir = Application.isPlaying ? GetMouseDirection() : Vector2.right;
        Vector2 point = (Vector2)attackPoint.position + mouseDir * 1f;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(point, 0.5f);
    }
}
