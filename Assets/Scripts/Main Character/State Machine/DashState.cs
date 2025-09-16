using UnityEngine;

public class DashState : IPlayerState
{
    private PlayerController ctx;
    private StateMachine sm;
    private Vector2 dashDir;
    private float dashTimer;

    public DashState(PlayerController context, StateMachine stateMachine)
    {
        ctx = context;
        sm = stateMachine;
    }

    public void Enter()
    {
        ctx.IsDashing = true;

        // 🔄 Usar el input directo (como antes)
        dashDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (dashDir == Vector2.zero) dashDir = ctx.lastNonZeroMoveInput; // fallback al último movimiento

        dashTimer = ctx.dashDuration;
        ctx.isInvulnerable = true;

        if (ctx.hitbox != null) ctx.hitbox.enabled = false;
        if (ctx.animator != null) ctx.animator.SetBool("isDashing", true);

        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Enemy"),
            true
        );

        AudioManager.Instance.Play("dash");
    }

    public void HandleInput() { }

    public void LogicUpdate() { }

    public void PhysicsUpdate()
    {
        ctx.rb.MovePosition(ctx.rb.position + dashDir * ctx.dashSpeed * Time.fixedDeltaTime);

        dashTimer -= Time.fixedDeltaTime;
        if (dashTimer <= 0f)
        {
            sm.ChangeState(ctx.IdleState);
        }
    }

    public void Exit()
    {
        ctx.rb.linearVelocity = Vector2.zero;

        if (ctx.hitbox != null) ctx.hitbox.enabled = true;

        ctx.isInvulnerable = false;
        ctx.IsDashing = false;

        if (ctx.animator != null) ctx.animator.SetBool("isDashing", false);

        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Enemy"),
            false
        );
    }
}
