using UnityEngine;
using System.Collections;

public class SpectralCompanion : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float fireInterval = 3f;
    public int damage = 1;
    public float speed = 5f;
    public Vector3 offset = new Vector3(-0.7f, -0.3f, 0f); // Separación estilo mascota

    private Transform player;
    private float fireTimer;

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

        // 🔫 Disparar si toca
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            Shoot();
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null) return;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        // Pasar el daño directamente al script de la bala
        SpectralBullet bulletScript = bullet.GetComponent<SpectralBullet>();
        if (bulletScript != null)
        {
            bulletScript.damage = damage;
        }
    }
}
