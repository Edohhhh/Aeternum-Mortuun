using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [Header("Configuración del Laser")]
    public float duration = 4f;
    public float damagePerSecond = 10f;
    public float damageInterval = 0.1f; // Más frecuente para mejor sensación
    public float maxDistance = 15f; // Distancia máxima del laser

    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    [Header("Visual")]
    public LineRenderer lineRenderer;
    public float lineWidth = 0.1f;
    public Gradient laserColor;
    public float pulseSpeed = 5f;
    public float pulseIntensity = 0.2f;

    private Transform playerTransform;
    private float timer;
    private float damageTimer;
    private bool isActive = false;
    private float baseWidth;
    private float startTime;

    private void Start()
    {
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        SetupLineRenderer();
        timer = duration;
        baseWidth = lineWidth;
        startTime = Time.time;
        isActive = true;
    }

    private void SetupLineRenderer()
    {
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.colorGradient = laserColor;
        lineRenderer.positionCount = 2;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        // Configurar para pixel art
        lineRenderer.alignment = LineAlignment.View;
        lineRenderer.textureMode = LineTextureMode.Tile;
    }

    public void Initialize(Transform player)
    {
        playerTransform = player;
        UpdateLaserVisual();
    }

    private void Update()
    {
        if (!isActive || playerTransform == null)
        {
            if (playerTransform == null) Deactivate();
            return;
        }

        timer -= Time.deltaTime;
        damageTimer += Time.deltaTime;

        // Animación de pulso del laser
        float pulse = Mathf.Sin((Time.time - startTime) * pulseSpeed) * pulseIntensity + 1f;
        lineRenderer.startWidth = baseWidth * pulse;
        lineRenderer.endWidth = baseWidth * pulse;

        // Aplicar daño a enemigos en la línea
        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;
            ApplyDamageToEnemies();
        }

        // Actualizar visual del laser (seguir al jugador y apuntar al mouse)
        UpdateLaserVisual();

        if (timer <= 0f)
        {
            Deactivate();
        }
    }

    private void UpdateLaserVisual()
    {
        if (lineRenderer != null && playerTransform != null)
        {
            Vector2 playerPosition = playerTransform.position;
            Vector2 mousePosition = GetMouseWorldPosition();
            Vector2 direction = (mousePosition - playerPosition).normalized;

            // Calcular punto final del laser (hasta maxDistance o hasta el primer obstáculo)
            Vector2 endPoint = playerPosition + (direction * maxDistance);

            // Raycast para ver si hay obstáculos
            RaycastHit2D hit = Physics2D.Raycast(playerPosition, direction, maxDistance);
            if (hit.collider != null && !hit.collider.CompareTag("Player"))
            {
                endPoint = hit.point;
            }

            lineRenderer.SetPosition(0, playerPosition);
            lineRenderer.SetPosition(1, endPoint);
        }
    }

    private void ApplyDamageToEnemies()
    {
        if (playerTransform == null) return;

        Vector2 playerPosition = playerTransform.position;
        Vector2 mousePosition = GetMouseWorldPosition();
        Vector2 direction = (mousePosition - playerPosition).normalized;
        float distance = maxDistance;

        // Raycast para encontrar el punto final real
        RaycastHit2D hit = Physics2D.Raycast(playerPosition, direction, maxDistance);
        if (hit.collider != null && !hit.collider.CompareTag("Player"))
        {
            distance = Vector2.Distance(playerPosition, hit.point);
        }

        RaycastHit2D[] hits = Physics2D.RaycastAll(playerPosition, direction, distance);
        foreach (RaycastHit2D rayHit in hits)
        {
            if (rayHit.collider != null && rayHit.collider.CompareTag("Enemy"))
            {
                EnemyHealth enemy = rayHit.collider.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    // Calcular daño por intervalo
                    int damage = Mathf.CeilToInt(damagePerSecond * damageInterval);

                    // Aplicar knockback similar a los ataques normales
                    Vector2 knockbackDir = direction;
                    float knockbackForceActual = knockbackForce;

                    enemy.TakeDamage(damage, knockbackDir, knockbackForceActual);

                    // Opcional: aplicar knockback continuo mientras el laser está activo
                    ApplyContinuousKnockback(enemy, knockbackDir);
                }
            }
        }
    }

    private void ApplyContinuousKnockback(EnemyHealth enemy, Vector2 direction)
    {
        // Opcional: aplicar una pequeña fuerza continua mientras el enemigo está en el laser
        // Esto crea un efecto de empuje constante
        /*
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            enemyRb.AddForce(direction * (knockbackForce * 0.1f), ForceMode2D.Force);
        }
        */
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        return new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);
    }

    private void Deactivate()
    {
        isActive = false;
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
        Destroy(gameObject, 0.1f);
    }
}