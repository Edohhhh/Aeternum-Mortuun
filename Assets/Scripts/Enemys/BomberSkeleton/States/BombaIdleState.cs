using UnityEngine;

public class BombaIdleState : State<EnemyInputs>
{
    private readonly Animator animator;
    private readonly Rigidbody2D rb;

    // Constructor de 3 argumentos para BombaController
    public BombaIdleState(BombaController ctrl, Animator anim, Rigidbody2D rigidBody)
    {
        animator = anim;
        rb = rigidBody;
    }

    public override void Awake()
    {
        base.Awake();

        if (animator != null)
            animator.SetBool("isWalking", false);

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}