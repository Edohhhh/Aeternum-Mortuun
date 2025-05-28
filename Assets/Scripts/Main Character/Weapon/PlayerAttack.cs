using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public AttackData[] comboAttacks;
    public Transform attackPoint;
    public LayerMask enemyLayers;
    public float animTime;

    public GameObject slashPrefab;
    public float slashLifetime = 0.5f;

    public int comboIndex = 0;
    private float lastAttackTime;
    private float comboResetTime = 1.0f;

    private PlayerController playerController;

    public Vector2 lastAttackDir { get; private set; }

    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        animTime += Time.deltaTime;

        Vector2 mouseDir = GetMouseDirection();

        float angle = Mathf.Atan2(mouseDir.y, mouseDir.x) * Mathf.Rad2Deg;
        attackPoint.rotation = Quaternion.Euler(0f, 0f, angle);
        attackPoint.position = transform.position + (Vector3)(mouseDir.normalized * 1f);

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

    public Vector2 GetMouseDirection()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        return ((Vector2)(mouseWorld - transform.position)).normalized;
    }

    void TryAttack(Vector2 dir)
    {
        animTime = 0f;
        lastAttackDir = dir;

        if (comboAttacks.Length == 0) return;

        var attack = comboAttacks[comboIndex];
        if (Time.time >= lastAttackTime + attack.cooldown)
        {
            lastAttackTime = Time.time;

            playerController.canMove = false;
            PerformAttack(attack, dir);

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

        if (slashPrefab != null)
        {
            GameObject slash = Instantiate(slashPrefab, attackPoint.position, Quaternion.identity);

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            slash.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            Vector3 scale = slash.transform.localScale;

            if (comboIndex % 3 == 1)
            {
                scale.y *= -1; // Flipear solo el segundo ataque
            }

            slash.transform.localScale = scale;

            Destroy(slash, slashLifetime);
        }

        Debug.Log("Performed attack: " + attack.attackName);
    }

    public string GetCurrentAttackType()
    {
        if (comboAttacks.Length == 0) return "";
        return comboAttacks[comboIndex].attackType;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || attackPoint == null)
            return;

        Vector2 dir = lastAttackDir != Vector2.zero ? lastAttackDir : Vector2.right;
        Vector2 attackPos = (Vector2)attackPoint.position + dir.normalized * comboAttacks[comboIndex].range;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos, comboAttacks[comboIndex].hitboxRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(attackPoint.position, attackPos);
    }
}