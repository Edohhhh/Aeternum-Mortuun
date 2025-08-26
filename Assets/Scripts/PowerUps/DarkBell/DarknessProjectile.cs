using UnityEngine;

public class DarknessProjectile : MonoBehaviour
{
    [Header("Configuración")]
    public float speed = 2f;
    public float rotationSpeed = 180f;
    public float lifetime = 10f;
    public float healAmount = 0.5f; // 0.5 de vida por defecto

    private Transform target;
    private PlayerController playerController;
    private SpriteRenderer spriteRenderer;
    private float timer;

    private void Start()
    {
        timer = lifetime;
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerController = target?.GetComponent<PlayerController>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.black;
        }

        // Destruir automáticamente después del tiempo de vida
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (target == null)
        {
            // Si no hay target, mover en línea recta
            transform.Translate(Vector2.right * speed * Time.deltaTime);
            return;
        }

        // Rotar el proyectil
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // Mover hacia el jugador
        Vector2 direction = (target.position - transform.position).normalized;
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        // Verificar distancia para curar
        float distance = Vector2.Distance(transform.position, target.position);
        if (distance < 0.5f)
        {
            HealPlayer();
            Destroy(gameObject);
        }
    }

    private void HealPlayer()
    {
        if (playerController != null)
        {
            PlayerHealth playerHealth = playerController.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.ModifyHealthFlat(healAmount);
                Debug.Log($"[DARKNESS PROJECTILE] Jugador curado +{healAmount} de vida");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Destruir al tocar obstáculos
        if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}