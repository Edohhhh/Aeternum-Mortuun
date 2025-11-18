using UnityEngine;

public class ProyectilEnemigo : MonoBehaviour
{
    private float speed;
    private Vector2 direction;
    public float damage = 0.5f; // El PDF dice "medio corazón"

    public void Initialize(Vector2 dir, float spd, float lifetime)
    {
        direction = dir.normalized;
        speed = spd;
        Destroy(gameObject, lifetime); // Se autodestruye después de 10s (según PDF)
    }

    void Update()
    {
        // Mover el proyectil
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    // Detección de colisión
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Busca al jugador (usando el método robusto)
        var playerHealth = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            // ¡Aplica daño! (usamos el TakeDamage con knockback)
            // Le ponemos un knockback muy pequeño
            playerHealth.TakeDamage(damage, transform.position, 1f, 0.1f);

            Destroy(gameObject); // Destruir el proyectil al golpear al jugador
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground")) // O la capa que uses para el escenario
        {
            // Opcional: crear un VFX de "flecha clavada"
            Destroy(gameObject); // Destruir el proyectil al chocar con el escenario
        }
    }
}