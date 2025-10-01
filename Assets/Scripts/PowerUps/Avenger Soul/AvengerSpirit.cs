using UnityEngine;

public class AvengerSpirit : MonoBehaviour
{
    public float speed = 4f;
    public int damage = 1;

    private Transform target;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        FindTarget();
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Mover hacia el target
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // Hacer que mire hacia la dirección del enemigo (izquierda o derecha)
        if (spriteRenderer != null)
        {
            // Comparar posiciones X para determinar la dirección
            if (target.position.x > transform.position.x)
            {
                // El enemigo está a la derecha, no hacer flip (mirar a la derecha)
                spriteRenderer.flipX = false;
            }
            else
            {
                // El enemigo está a la izquierda, hacer flip (mirar a la izquierda)
                spriteRenderer.flipX = true;
            }
        }
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