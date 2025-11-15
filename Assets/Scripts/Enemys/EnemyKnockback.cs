using UnityEngine;
using System.Collections;

public class EnemyKnockback : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 knockbackDirection; // Dirección del knockback
    private bool isKnockedBack = false; // Estado de knockback
    private float knockbackTimer = 0f; // Temporizador para la duración del knockback
    public float knockbackDuration = 0.5f; // Duración del knockback
    public float knockbackForce = 5f; // Fuerza del knockback
    public float knockbackDecay = 0.5f; // Desaceleración del knockback (factor de desaceleración)

    // **Cooldown** añadido
    public float knockbackCooldown = 1f; // Tiempo de espera antes de poder recibir knockback nuevamente
    private bool canReceiveKnockback = true; // Controla el cooldown del knockback

    public bool IsKnockedBack => isKnockedBack; // Propiedad pública para comprobar el estado del knockback

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isKnockedBack)
        {
            // Durante el knockback, aplicamos desaceleración
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer > 0)
            {
                rb.linearVelocity = knockbackDirection * (knockbackTimer / knockbackDuration); // Disminuye la velocidad
            }
            else
            {
                isKnockedBack = false; // Terminamos el knockback cuando se acabe el tiempo
            }
        }
    }

    // Método para aplicar knockback con cooldown
    public void ApplyKnockback(Vector2 knockbackDir)
    {
        if (canReceiveKnockback)
        {
            knockbackDirection = knockbackDir * knockbackForce;
            knockbackTimer = knockbackDuration; // Restablece el temporizador de knockback
            isKnockedBack = true;

            // También podemos restablecer la velocidad a cero antes de aplicar el knockback, si es necesario
            rb.linearVelocity = Vector2.zero;

            // Inicia el cooldown de knockback
            StartCoroutine(KnockbackCooldown());
        }
    }

    // Coroutine para el cooldown del knockback
    private IEnumerator KnockbackCooldown()
    {
        canReceiveKnockback = false; // Desactivar el knockback temporalmente

        // Espera el tiempo de cooldown
        yield return new WaitForSeconds(knockbackCooldown);

        canReceiveKnockback = true; // Permitir nuevamente el knockback después del cooldown
    }
}
