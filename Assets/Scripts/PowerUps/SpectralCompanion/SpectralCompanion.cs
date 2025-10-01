using UnityEngine;
using System.Collections;

public class SpectralCompanion : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float fireInterval = 3f;
    public int damage = 1;
    public float speed = 5f;
    public float followDistance = 1.0f; // Distancia detrás del jugador

    private Transform player;
    private SpriteRenderer playerSpriteRenderer; // Para saber hacia dónde mira
    private float fireTimer;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Transform playerTransform, float moveSpeed, float interval, int bulletDamage)
    {
        player = playerTransform;
        playerSpriteRenderer = player.GetComponent<SpriteRenderer>();

        speed = moveSpeed;
        fireInterval = interval;
        damage = bulletDamage;
    }

    void Update()
    {
        if (player == null)
        {
            var foundPlayer = GameObject.FindWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
                playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
            }
            else
            {
                return;
            }
        }

        // 🧭 Calcular posición DETRÁS del jugador
        Vector3 behindPosition = CalculateBehindPosition();

        // 🐾 Moverse suavemente hacia esa posición
        transform.position = Vector3.MoveTowards(transform.position, behindPosition, speed * Time.deltaTime);

        // 👀 Mirar hacia el enemigo más cercano (opcional, como antes)
        Transform closestEnemy = FindClosestEnemy();
        if (closestEnemy != null)
        {
            LookAtTarget(closestEnemy.position);
        }

        // 🔫 Disparar
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            Shoot(closestEnemy);
        }
    }

    private Vector3 CalculateBehindPosition()
    {
        if (playerSpriteRenderer == null)
            return player.position; // fallback

        // Determinar dirección hacia adelante del jugador
        float facingDirection = playerSpriteRenderer.flipX ? -1f : 1f;

        // Posición detrás = posición del jugador - (dirección * distancia)
        Vector3 behindOffset = new Vector3(-facingDirection * followDistance, 0f, 0f);
        return player.position + behindOffset;
    }

    private Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return null;

        Transform closest = null;
        float closestDist = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = enemy.transform;
            }
        }
        return closest;
    }

    private void LookAtTarget(Vector3 targetPosition)
    {
        if (spriteRenderer == null) return;

        float direction = targetPosition.x - transform.position.x;
        spriteRenderer.flipX = direction < 0; // true = mira izquierda
    }

    private void Shoot(Transform targetEnemy)
    {
        if (bulletPrefab == null) return;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        SpectralBullet bulletScript = bullet.GetComponent<SpectralBullet>();
        if (bulletScript != null)
        {
            bulletScript.damage = damage;
        }

        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }
    }
}