using UnityEngine;
using System.Collections;

public class BombaDeathExplosionState : State<EnemyInputs>
{
    private readonly BombaController controller;
    private readonly Animator animator;
    private readonly Rigidbody2D rb;

    public BombaDeathExplosionState(BombaController ctrl, Animator anim, Rigidbody2D rigidBody)
    {
        controller = ctrl;
        animator = anim;
        rb = rigidBody;
    }

    public override void Awake()
    {
        base.Awake();

        // 1. Detiene el timer principal de 5s (y el parpadeo)
        controller.StopExplosionTimer();

        // 2. Se queda quieto (Requisito 1: "que quede quieto")
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            // Lo hacemos Kinematic para que nada (ni el jugador) lo mueva
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // 3. Desactiva TODOS los colliders del enemigo (Requisito 2: "desactive las colisiones")
        // Esto incluye el collider físico Y el trigger de standoff
        Collider2D[] allColliders = controller.GetComponents<Collider2D>();
        foreach (Collider2D col in allColliders)
        {
            col.enabled = false;
        }

        // 4. Reproduce la animación de muerte
        if (animator != null) animator.SetTrigger("Die");

        // 5. Inicia la nueva cuenta atrás de 2 segundos (Requisito 3: "resetee el contador")
        controller.StartCoroutine(DelayedExplosion());
    }

    private IEnumerator DelayedExplosion()
    {
        // Espera los 2 segundos que dice el PDF (definidos en 'deathExplosionDelay' 
        // en el Inspector del BombaController)
        yield return new WaitForSeconds(controller.deathExplosionDelay);

        // Llama a la explosión (que crea el VFX y destruye al enemigo)
        controller.PerformExplosion();
    }
}