using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class AttackHitbox : MonoBehaviour
{
    public int damage = 1;
    public float lifeTime = 0.1f;
    public Vector2 knockbackDir;
    public float knockbackForce = 100f;

    private void OnEnable()
    {
        // 1) Si la hitbox está como hijo del Player (recomendado)
        var owner = GetComponentInParent<PlayerController>();

        // 2) Fallback: si no está parented al Player, buscá por tag "Player"
        if (owner == null)
        {
            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo != null) owner = playerGo.GetComponent<PlayerController>();
        }

        // Snapshot del daño actual del Player en el instante del spawn
        if (owner != null)
            damage = Mathf.Max(1, owner.baseDamage);
    }

    private void Start()
    {
        // Se destruye usando el lifetime que tengas seteado (prefab o spawner)
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        var enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, knockbackDir, knockbackForce);
        }
    }
}
