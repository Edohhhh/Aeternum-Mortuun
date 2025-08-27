using UnityEngine;
using System.Collections;

public class CursedCompanion : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float fireInterval = 5f; // Mayor cooldown entre ráfagas
    public float burstInterval = 0.1f; // Tiempo entre cada bala en la ráfaga
    public int bulletsPerBurst = 3; // Número de balas por ráfaga
    public int damage = 1;
    public float speed = 5f;
    public Vector3 offset = new Vector3(-0.7f, -0.3f, 0f); // Separación estilo mascota

    private Transform player;
    private float fireTimer;
    private bool isFiring = false;

    public void Initialize(Transform playerTransform, float moveSpeed, float interval, int bulletDamage)
    {
        player = playerTransform;
        speed = moveSpeed;
        fireInterval = interval;
        damage = bulletDamage;
    }

    void Update()
    {
        // 🧍 Si el jugador desapareció (por cambio de escena)
        if (player == null)
        {
            var foundPlayer = GameObject.FindWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
            else
            {
                return;
            }
        }

        // 🐾 Moverse hacia la posición offset del jugador
        Vector3 targetPosition = player.position + offset;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // 🔫 Disparar si toca y no está ya disparando
        if (!isFiring)
        {
            fireTimer += Time.deltaTime;
            if (fireTimer >= fireInterval)
            {
                fireTimer = 0f;
                StartCoroutine(FireBurst());
            }
        }
    }

    private IEnumerator FireBurst()
    {
        isFiring = true;

        // Disparar las balas en ráfaga
        for (int i = 0; i < bulletsPerBurst; i++)
        {
            ShootProjectile(i);

            // Esperar un poco antes de la siguiente bala
            if (i < bulletsPerBurst - 1) // No esperar después de la última
            {
                yield return new WaitForSeconds(burstInterval);
            }
        }

        isFiring = false;
        Debug.Log($"[CURSED COMPANION] Ráfaga de {bulletsPerBurst} proyectiles disparada!");
    }

    private void ShootProjectile(int bulletIndex)
    {
        if (bulletPrefab == null) return;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        // Calcular el ángulo de desvío para cada bala
        float angleOffset = 0f;
        if (bulletsPerBurst > 1)
        {
            // Distribuir las balas en un arco
            float totalSpread = 30f; // Grados totales de separación
            float spacing = totalSpread / (bulletsPerBurst - 1);
            angleOffset = -totalSpread / 2f + (bulletIndex * spacing);
        }

        // Pasar datos al script de la bala
        CursedBullet bulletScript = bullet.GetComponent<CursedBullet>();
        if (bulletScript != null)
        {
            bulletScript.damage = damage;
            bulletScript.angleOffset = angleOffset;
        }
        else
        {
            // Si no tiene CursedBullet, usar el SpectralBullet existente
            SpectralBullet spectralBullet = bullet.GetComponent<SpectralBullet>();
            if (spectralBullet != null)
            {
                spectralBullet.damage = damage;
            }
        }
    }
}