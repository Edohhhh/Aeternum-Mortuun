using UnityEngine;

public class KnockbackState : IPlayerState
{
    private PlayerController ctx;
    private StateMachine sm;
    private float knockbackTimer;
    private float knockbackDuration = 0.2f;

    private Vector2 knockDirection;
    private float knockForce = 10f;

    public KnockbackState(PlayerController context, StateMachine stateMachine)
    {
        ctx = context;
        sm = stateMachine;
    }

    public void Enter()
    {
        ctx.animator.SetTrigger("Hit");
        ctx.rb.linearVelocity = Vector2.zero;
        ctx.rb.AddForce(knockDirection * knockForce, ForceMode2D.Impulse);
        knockbackTimer = 0f;
    }

    public void SetKnockback(Vector2 direction, float force = 10f, float duration = 0.2f)
    {
     
        if (ctx.isInvulnerable)
            return;

        knockDirection = direction.normalized;
        knockForce = force;
        knockbackDuration = duration;
    }

    public void HandleInput() { }

    public void LogicUpdate()
    {
        knockbackTimer += Time.deltaTime;
        if (knockbackTimer >= knockbackDuration)
        {
            sm.ChangeState(ctx.IdleState);
        }
    }

    public void PhysicsUpdate() { }

    public void Exit()
    {
        ctx.rb.linearVelocity = Vector2.zero; // Detiene el knockback completamente
    }
}