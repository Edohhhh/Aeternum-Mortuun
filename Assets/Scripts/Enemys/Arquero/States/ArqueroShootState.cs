using UnityEngine;

public class ArqueroShootState : State<EnemyInputs>
{
    private readonly ArqueroController controller;
    private readonly Animator animator;

    public ArqueroShootState(ArqueroController ctrl, Animator anim)
    {
        controller = ctrl;
        animator = anim;
    }

    public override void Awake()
    {
        base.Awake();

        // 1. Marcar el Cooldown como usado
        controller.MarkShootAsUsed();

        // 2. Disparar la animación de ataque
        animator?.SetTrigger("Shoot");

        // 3. Volver a Idle inmediatamente
        // El Árbol de Decisión no se ejecutará (gracias al flag IsShooting())
        // hasta que este estado termine.
        // La animación de "Shoot" se reproduce, y el evento 'FireProjectile'
        // se disparará desde el controller.
    }

    public override void Execute()
    {
        // Podríamos esperar a que la animación termine, pero
        // para un enemigo estático, es más simple volver a Idle.
        // El Árbol de Decisión no volverá a disparar gracias al Cooldown.

        // Si tu animación "Shoot" NO transiciona sola a Idle,
        // necesitas una transición aquí.

        // Transición simple:
        controller.Transition(EnemyInputs.LostPlayer); // Volver a Idle
    }
}