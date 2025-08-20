using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OrbitalSoul : MonoBehaviour
{
    [Header("Configuración de órbita")]
    public float orbitRadius = 1.2f;
    public float rotationSpeed = 90f; // grados por segundo

    [Header("Configuración de daño por contacto")]
    public int contactDamage = 1;
    public float contactDamageInterval = 1f;

    [Header("Configuración de proyectiles")]
    public GameObject bulletPrefab;
    public float shootInterval = 2f;
    public float bulletSpeed = 5f;
    public int bulletDamage = 1;

    private Transform target;
    private float angle;
    private float contactDamageTimer;
    private float shootTimer;
    private Dictionary<EnemyHealth, float> lastContactDamageTimes = new Dictionary<EnemyHealth, float>();

    public void Initialize(Transform target, float radius, float speed, int contactDmg, float contactInterval,
                          GameObject bullet, float shootInt, float bulletSpd, int bulletDmg)
    {
        this.target = target;
        orbitRadius = radius;
        rotationSpeed = speed;
        contactDamage = contactDmg;
        contactDamageInterval = contactInterval;
        bulletPrefab = bullet;
        shootInterval = shootInt;
        bulletSpeed = bulletSpd;
        bulletDamage = bulletDmg;
    }

    void Update()
    {
        if (target == null) return;

        // Rotar alrededor del jugador
        angle += rotationSpeed * Time.deltaTime;
        float radians = angle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * orbitRadius;
        transform.position = target.position + offset;

        // Daño por contacto
        contactDamageTimer += Time.deltaTime;
        if (contactDamageTimer >= contactDamageInterval)
        {
            contactDamageTimer = 0f;
            DamageEnemiesByContact();
        }

        // Disparar proyectiles
        shootTimer += Time.deltaTime;
        if (shootTimer >= shootInterval)
        {
            shootTimer = 0f;
            ShootAtNearestEnemy();
        }
    }

    private void DamageEnemiesByContact()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.3f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                var enemy = hit.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    // Verificar cooldown de daño por contacto
                    float currentTime = Time.time;
                    if (!lastContactDamageTimes.ContainsKey(enemy) ||
                        currentTime - lastContactDamageTimes[enemy] >= contactDamageInterval)
                    {
                        Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                        float knockbackForce = 0f;
                        enemy.TakeDamage(contactDamage, knockbackDir, knockbackForce);

                        lastContactDamageTimes[enemy] = currentTime;
                    }
                }
            }
        }
    }

    private void ShootAtNearestEnemy()
    {
        if (bulletPrefab == null) return;

        // Buscar enemigo más cercano
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (var e in enemies)
        {
            float dist = Vector2.Distance(transform.position, e.transform.position);
            if (dist < closestDist && dist <= 10f) // Limitar rango de disparo
            {
                closestDist = dist;
                closest = e.transform;
            }
        }

        if (closest != null)
        {
            // Crear proyectil
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            SpectralBullet bulletComponent = bullet.GetComponent<SpectralBullet>();

            if (bulletComponent != null)
            {
                // Configurar el proyectil con nuestros valores
                bulletComponent.speed = bulletSpeed;
                bulletComponent.damage = bulletDamage;
            }
            else
            {
                // Si no tiene el componente, destruirlo para evitar errores
                Destroy(bullet);
                return;
            }

            // El SpectralBullet ya se encarga de buscar el target, pero podemos ayudarlo
            // Opcional: podrías modificar el script para aceptar un target específico
        }
    }

    private void OnDestroy()
    {
        lastContactDamageTimes.Clear();
    }
}
