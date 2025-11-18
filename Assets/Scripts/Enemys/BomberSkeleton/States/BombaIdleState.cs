using UnityEngine;

public class BombaIdleState : State<EnemyInputs>
{
    private readonly Animator animator;
    private readonly Rigidbody2D rb;

    public BombaIdleState(BombaController ctrl, Animator anim, Rigidbody2D rigidBody)
    {
        animator = anim;
        rb = rigidBody;
    }

    public override void Awake()
    {
        base.Awake();
        // Debug.Log("BOMBA IdleState: FRENANDO"); // (Puedes descomentar esto para probar)
        if (animator != null)
            animator.SetBool("isWalking", false);
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    // ESTA PARTE ES CRUCIAL para el "freno en seco"
    public override void FixedExecute()
    {
        // Forzamos la velocidad a CERO en cada ciclo de físicas
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}