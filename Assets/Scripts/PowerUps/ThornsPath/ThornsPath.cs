using UnityEngine;

public class ThornsPath : MonoBehaviour
{
    [Header("Configuración del ThornsPath")]
    public float duration = 4f;
    public float damagePerSecond = 10f;
    public float damageInterval = 0.5f;

    [Header("Visual")]
    public LineRenderer lineRenderer;
    public float lineWidth = 0.1f;
    public Gradient thornsPathColor;

    private Vector2 startPosition;
    private Vector2 endPosition;
    private float timer;
    private float damageTimer;
    private bool isActive = false;

    private void Start()
    {
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        SetupLineRenderer();
        timer = duration;
        isActive = true;
    }

    private void SetupLineRenderer()
    {
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.colorGradient = thornsPathColor;
        lineRenderer.positionCount = 2;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Material básico para pixel art
    }

    public void Initialize(Vector2 start, Vector2 end)
    {
        startPosition = start;
        endPosition = end;
        UpdateThornsPathVisual();
    }

    private void Update()
    {
        if (!isActive) return;

        timer -= Time.deltaTime;
        damageTimer += Time.deltaTime;

        UpdateThornsPathVisual();

        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;
            ApplyDamageToEnemies();
        }

        if (timer <= 0f)
        {
            Deactivate();
        }
    }

    private void UpdateThornsPathVisual()
    {
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, endPosition);
        }
    }

    private void ApplyDamageToEnemies()
    {
        Vector2 direction = (endPosition - startPosition).normalized;
        float distance = Vector2.Distance(startPosition, endPosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(startPosition, direction, distance);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    int damage = Mathf.CeilToInt(damagePerSecond * damageInterval);
                    Vector2 knockbackDir = direction;
                    float knockbackForce = 2f;
                    enemy.TakeDamage(damage, knockbackDir, knockbackForce);
                }
            }
        }
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