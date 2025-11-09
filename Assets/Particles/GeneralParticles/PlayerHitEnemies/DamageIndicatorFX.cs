using UnityEngine;
using TMPro;

public class DamageNumberFX : MonoBehaviour
{
    [Header("Referencia al TMP en el prefab")]
    [SerializeField] private TextMeshPro tmp;

    [Header("Comportamiento")]
    [SerializeField] private float lifetime = 0.9f;
    [SerializeField] private float riseSpeed = 0.9f;
    [SerializeField] private float outImpulse = 1.2f;
    [SerializeField] private float damping = 3.5f;
    [SerializeField] private Vector2 randomJitter = new Vector2(0.15f, 0.05f);

    [Header("Render (opcional)")]
    [SerializeField] private string sortingLayerName = "Effects";
    [SerializeField] private int sortingOrder = 200;

    [Header("Color según daño")]
    [SerializeField] private Color minDamageColor = Color.yellow;
    [SerializeField] private Color maxDamageColor = Color.red;
    [SerializeField] private float maxDamageValue = 10f; // a partir de este daño el color será rojo

    private Vector2 velocity;
    private float t;

    void Reset()
    {
        tmp = GetComponentInChildren<TextMeshPro>();
    }

    public void Init(int damage, Vector2 hitDir)
    {
        if (!tmp) tmp = GetComponentInChildren<TextMeshPro>();

        // Texto
        tmp.text = damage.ToString();

        // Color según daño (interpolación entre amarillo y rojo)
        float normalizedDamage = Mathf.Clamp01(damage / maxDamageValue);
        tmp.color = Color.Lerp(minDamageColor, maxDamageColor, normalizedDamage);

        // Dirección contraria al golpe + jitter
        Vector2 outDir = (-hitDir).normalized;
        Vector2 jitter = new Vector2(
            Random.Range(-randomJitter.x, randomJitter.x),
            Random.Range(-randomJitter.y, randomJitter.y)
        );

        velocity = outDir * outImpulse + jitter;

        // Render order
        var rend = tmp.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.sortingLayerName = sortingLayerName;
            rend.sortingOrder = sortingOrder;
        }

        // Empezar vida
        t = 0f;
        enabled = true;
    }

    void OnEnable()
    {
        if (!tmp) tmp = GetComponentInChildren<TextMeshPro>();
    }

    void Update()
    {
        float dt = Time.deltaTime;
        t += dt;

        transform.position += (Vector3)(velocity * dt);
        velocity = Vector2.Lerp(velocity, Vector2.zero, damping * dt);
        transform.position += Vector3.up * (riseSpeed * dt);

        if (tmp)
        {
            float life01 = Mathf.Clamp01(t / lifetime);
            float alpha = 1f - Mathf.SmoothStep(0.6f, 1f, life01);
            var c = tmp.color;
            c.a = alpha;
            tmp.color = c;
        }

        if (t >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
