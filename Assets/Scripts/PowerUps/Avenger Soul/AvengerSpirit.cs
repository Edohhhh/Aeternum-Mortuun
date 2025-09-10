using UnityEngine;

public class AvengerSpirit : MonoBehaviour
{
    public float speed = 4f;
    public int damage = 1;

    private Transform target;

    private void Start()
    {
        FindTarget();
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
    }

    private void FindTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length > 0)
        {
            target = enemies[Random.Range(0, enemies.Length)].transform;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, Vector2.zero, 0f);
            }

            Destroy(gameObject);
        }
    }
}
