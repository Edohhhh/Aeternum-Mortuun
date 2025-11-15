using UnityEngine;

public class EnemyAITest : MonoBehaviour
{
    public Transform player;  // Referencia al jugador
    public float moveSpeed = 3f;  // Velocidad a la que se mueve el enemigo

    private Rigidbody2D rb;
    private Vector2 direction;
    private EnemyKnockback enemyKnockback; // Referencia al script de Knockback

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyKnockback = GetComponent<EnemyKnockback>(); // Obtener referencia al script de knockback

        // Asegurarse de que el jugador se haya asignado en el inspector
        if (player == null)
        {
            Debug.LogError("Player reference is missing in EnemyAITest script.");
        }
    }

    void Update()
    {
        if (player != null && !enemyKnockback.IsKnockedBack) // Solo seguir al jugador si no está en knockback
        {
            // Calcula la dirección hacia el jugador
            direction = (player.position - transform.position).normalized;
        }
    }

    void FixedUpdate()
    {
        if (!enemyKnockback.IsKnockedBack) // Si no está siendo afectado por knockback
        {
            rb.linearVelocity = direction * moveSpeed; // Mueve al enemigo hacia el jugador
        }
    }
}