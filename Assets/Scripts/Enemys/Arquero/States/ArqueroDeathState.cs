using UnityEngine;
using System.Collections;

public class ArqueroDeathState : State<EnemyInputs>
{
    private readonly ArqueroController controller;
    private readonly Animator animator;
    private readonly Rigidbody2D rb;

    public ArqueroDeathState(ArqueroController ctrl, Animator anim, Rigidbody2D rigidBody)
    {
        controller = ctrl;
        animator = anim;
        rb = rigidBody;
    }

    public override void Awake()
    {
        base.Awake();

        // Detener movimiento
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Reproducir animación de muerte
        if (animator != null) animator.SetTrigger("Die");

        // Desactivar colliders
        Collider2D collider = controller.GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;

        // --- LÍNEA CORREGIDA ---
        // Llamamos a la corutina para que se destruya después de un tiempo
        controller.StartCoroutine(DelayedDestroy());
    }

    // --- BLOQUE CORREGIDO ---
    // Esta corutina espera 3 segundos (puedes cambiar el tiempo)
    // y luego destruye el GameObject del arquero.
    private IEnumerator DelayedDestroy()
    {
        // Espera 3 segundos (o lo que dure tu animación de muerte)
        yield return new WaitForSeconds(3f);

        // Destruye el objeto
        if (controller != null)
        {
            GameObject.Destroy(controller.gameObject);
        }
    }
}