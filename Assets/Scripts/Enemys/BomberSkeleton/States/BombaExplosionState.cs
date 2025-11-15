using UnityEngine;

public class BombaExplosionState : State<EnemyInputs>
{
    private readonly BombaController controller;
    private readonly Animator animator;
    private readonly Rigidbody2D rb;

    public BombaExplosionState(BombaController ctrl, Animator anim, Rigidbody2D rigidBody)
    {
        controller = ctrl;
        animator = anim;
        rb = rigidBody;
    }

    public override void Awake()
    {
        base.Awake();
        controller.StopExplosionTimer();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (animator != null) animator.SetTrigger("Explode");
        controller.PerformExplosion();
    }
}