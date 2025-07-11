using UnityEngine;

public class PlayerAcidTrail : MonoBehaviour
{
    [Header("Daño global")]
    public float damage = 0.5f;
    public float knockbackForce = 0f;
    public Vector2 knockbackDir = Vector2.zero;
    public float globalDamageInterval = 3f;

    [Header("Ácido")]
    [SerializeField] private float lifetime = 3f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy == null) return;

        
        if (AcidDamageTracker.CanDamage(enemy, globalDamageInterval))
        {
            int finalDamage = Mathf.CeilToInt(damage);
            enemy.TakeDamage(finalDamage, knockbackDir, knockbackForce);
            Debug.Log($"[ACID-GLOBAL] {enemy.name} dañado con {finalDamage}. Cooldown global activo.");
        }
        else
        {
            Debug.Log($"[ACID-GLOBAL] {enemy.name} ignorado por cooldown global.");
        }
    }

    private void OnDestroy()
    {
        // Limpieza opcional si querés evitar residuos
        // AcidDamageTracker.ClearAll(); <- si implementás esa función
    }
}
