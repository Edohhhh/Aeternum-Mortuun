using UnityEngine;
using System.Collections; // Necesario para Corutinas

public class VFXSelfDestroy : MonoBehaviour
{
    private CircleCollider2D explosionCollider;

    // Puedes ajustar estos valores desde el Inspector del prefab del VFX
    public float initialRadius = 0.5f;   // Radio inicial del collider (pequeño)
    public float finalRadius = 3.0f;     // Radio final de la explosión
    public float growthDuration = 0.5f;  // Duración en segundos para crecer el collider

    [Header("Damage")]
    [Tooltip("La cantidad de daño que hace esta explosión")]
    public float damageAmount = 10f;
    void Awake()
    {
        explosionCollider = GetComponent<CircleCollider2D>();
        if (explosionCollider == null)
        {
            Debug.LogWarning("VFXSelfDestroy: No se encontró CircleCollider2D en " + gameObject.name);
            return;
        }

        // Asegurarse de que sea un trigger y no bloquee físicamente
        explosionCollider.isTrigger = true;

        // Inicializar el radio a cero o al valor inicial configurado
        explosionCollider.radius = initialRadius;

        // Desactivar el collider al inicio para que no haga daño hasta que se active
        explosionCollider.enabled = false;
    }

    // --- Nuevos métodos para controlar el collider ---

    // Llamado por un Evento de Animación para iniciar el crecimiento
    public void StartExplosionGrowth()
    {
        if (explosionCollider != null)
        {
            explosionCollider.enabled = true; // Activar el collider
            StartCoroutine(GrowColliderCoroutine());
        }
        else
        {
            Debug.LogWarning("VFXSelfDestroy: No se puede iniciar el crecimiento del collider, no se encontró CircleCollider2D.");
        }
    }

    // Corutina para aumentar el tamaño del collider suavemente
    private IEnumerator GrowColliderCoroutine()
    {
        float timer = 0f;
        while (timer < growthDuration)
        {
            timer += Time.deltaTime;
            float currentRadius = Mathf.Lerp(initialRadius, finalRadius, timer / growthDuration);
            explosionCollider.radius = currentRadius;
            yield return null; // Espera al siguiente frame
        }
        explosionCollider.radius = finalRadius; // Asegurarse de que llegue al tamaño final

        // Opcional: Desactivar o destruir el collider después de alcanzar el tamaño máximo
        // Por ahora lo dejamos activo para que la animación de explosión termine.
        // La autodestrucción la maneja el DestroySelf() al final de la animación.
    }

    // --- Tu método original de autodestrucción (llamado por Anim Event al final) ---
    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    // --- Detectar qué golpea la explosión ---
    void OnTriggerEnter2D(Collider2D other)
    {
        // Busca 'PlayerHealth' en el objeto golpeado O en su padre
        // (Igual que en tu script EnemyAttack, esto es más robusto)
        var playerHealth = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            // Aplica el daño
            playerHealth.TakeDamage(damageAmount, transform.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 1. Dibuja el radio inicial (ej. en amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, initialRadius);

        // 2. Dibuja el radio final (ej. en rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, finalRadius);
    }
}
