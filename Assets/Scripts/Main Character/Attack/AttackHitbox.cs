using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public int damage;
    public float lifeTime = 0.15f;
    public float hitStopDuration = 0.08f;
    public float knockbackForce = 15f;

    private CombatSystem combatRef;
    private Vector2 knockbackDir;

    public void Initialize(CombatSystem combat, Vector2 dir, int dmg)
    {
        combatRef = combat;
        knockbackDir = dir;
        damage = dmg;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, knockbackDir, knockbackForce);

                if (combatRef != null)
                {
                    combatRef.TriggerHitStop(hitStopDuration);
                    // ✅ AVISAR QUE GOLPEAMOS (Para el retroceso hacia atrás)
                    combatRef.OnAttackHit();
                }
            }
        }
    }
}