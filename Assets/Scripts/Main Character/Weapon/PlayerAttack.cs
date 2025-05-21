using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public AttackData[] comboAttacks;
    public Transform attackPoint;
    public LayerMask enemyLayers;

    public GameObject slashPrefab;
    public float slashLifetime = 0.5f;

    private int comboIndex = 0;
    private float lastAttackTime;
    private float comboResetTime = 1.0f;

    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        Vector2 mouseDir = GetMouseDirection();

        float angle = Mathf.Atan2(mouseDir.y, mouseDir.x) * Mathf.Rad2Deg;
        attackPoint.rotation = Quaternion.Euler(0f, 0f, angle);

        float distance = 1f;
        attackPoint.position = transform.position + (Vector3)(mouseDir.normalized * distance);

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
            lastAttackTime = Time.time;

            // Desactivar movimiento
            playerController.canMove = false;

            PerformAttack(attack, dir);

            // Reactivar movimiento después de la duración
            StartCoroutine(EndAttackAfterTime(playerController.attackDuration));

            comboIndex = (comboIndex + 1) % comboAttacks.Length;
        }
    }

    IEnumerator EndAttackAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        playerController.canMove = true;
    }

    void PerformAttack(AttackData attack, Vector2 dir)
    {
        Vector2 attackPosition = (Vector2)attackPoint.position + dir.normalized * attack.range;

        Debug.Log("Punto de ataque: " + attackPosition + " | Radio: " + attack.hitboxRadius);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPosition, attack.hitboxRadius, enemyLayers);
        Debug.Log("Enemigos detectados: " + hitEnemies.Length);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy == null) continue;

            Debug.Log("Colisionado con: " + enemy.name);

            var health = enemy.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.TakeDamage((int)attack.damage);
            }
        }

        // SPAWNEAR SLASH
        if (slashPrefab != null)
        {
            GameObject slash = Instantiate(slashPrefab, attackPoint.position, Quaternion.identity);

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            slash.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            if (dir.x < 0)
            {
                Vector3 scale = slash.transform.localScale;
                scale.y *= -1;
                slash.transform.localScale = scale;
            }

            Destroy(slash, slashLifetime);
        }

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
