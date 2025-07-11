using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerOrbital : MonoBehaviour
{
    private Transform player;
    private float radius;
    private float speed;
    private int damage;
    private float angle;

    private Dictionary<GameObject, float> damageCooldowns = new();
    private float damageInterval = 1f;

    public void Initialize(Transform playerTarget, float orbitRadius, float rotationSpeed, int damagePerSec)
    {
        player = playerTarget;
        radius = orbitRadius;
        speed = rotationSpeed;
        damage = damagePerSec;
    }

    private void Update()
    {
        if (player == null) return;

        angle += speed * Time.deltaTime;
        float rad = angle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        transform.position = (Vector2)player.position + offset;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        GameObject enemyGO = other.gameObject;
        if (!damageCooldowns.ContainsKey(enemyGO))
            damageCooldowns[enemyGO] = 0f;

        damageCooldowns[enemyGO] -= Time.deltaTime;
        if (damageCooldowns[enemyGO] <= 0f)
        {
            damageCooldowns[enemyGO] = damageInterval;

            var health = enemyGO.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(damage, transform.position, 0f);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (damageCooldowns.ContainsKey(other.gameObject))
            damageCooldowns.Remove(other.gameObject);
    }
}
