using UnityEngine;
using System.Collections;

public class SpectralCompanion : MonoBehaviour
{
    private Transform player;
    private float moveSpeed;
    private float fireInterval;
    private int damage;

    public GameObject bulletPrefab;
    public float bulletSpeed = 6f;

    public void Initialize(Transform target, float speed, float interval, int dmg)
    {
        player = target;
        moveSpeed = speed;
        fireInterval = interval;
        damage = dmg;

        StartCoroutine(ShootLoop());
    }

    private void Update()
    {
        if (player == null) return;

        Vector2 dir = (player.position - transform.position);
        float distance = dir.magnitude;

        float followDistance = 1.2f; // distancia mínima al player

        if (distance > followDistance)
        {
            Vector2 moveDir = dir.normalized;
            transform.position += (Vector3)(moveDir * moveSpeed * Time.deltaTime);
        }
    }

    private IEnumerator ShootLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(fireInterval);

            GameObject enemy = FindClosestEnemy();
            if (enemy != null && bulletPrefab != null)
            {
                Vector2 dir = (enemy.transform.position - transform.position).normalized;
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                var spectralBullet = bullet.AddComponent<SpectralBullet>();
                spectralBullet.Initialize(dir, bulletSpeed, damage);
            }
        }
    }

    private GameObject FindClosestEnemy()
    {
        float minDist = float.MaxValue;
        GameObject closest = null;

        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }

        return closest;
    }
}
