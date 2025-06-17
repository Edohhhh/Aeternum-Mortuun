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
        // Dirección del dash (si no hay input, va a la derecha)
        dashDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (dashDir == Vector2.zero) dashDir = Vector2.right;

        dashTimer = ctx.dashDuration;

        // Activar invulnerabilidad
        ctx.isInvulnerable = true;

        // Desactivar el hitbox para evitar daño
        if (ctx.hitbox != null)
            ctx.hitbox.enabled = false;

        // Activar animación de dash
        if (ctx.animator != null)
            ctx.animator.SetBool("isDashing", true);

        // (Opcional) Ignorar colisiones con enemigos
        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Enemy"),
            true
        );
    }

    public void HandleInput() { }

    public void LogicUpdate() { }

    public void PhysicsUpdate()
    {
        // Mover al jugador
        ctx.rb.MovePosition(ctx.rb.position + dashDir * ctx.dashSpeed * Time.fixedDeltaTime);

        // Contador del dash
        dashTimer -= Time.fixedDeltaTime;
        if (dashTimer <= 0f)
        {
            sm.ChangeState(ctx.IdleState);
        }
    }

    public void Exit()
    {
        // Detener movimiento
        ctx.rb.linearVelocity = Vector2.zero;

        // Restaurar el hitbox
        if (ctx.hitbox != null)
            ctx.hitbox.enabled = true;

        // Desactivar invulnerabilidad
        ctx.isInvulnerable = false;

        // Desactivar animación de dash
        if (ctx.animator != null)
            ctx.animator.SetBool("isDashing", false);

        // Restaurar colisiones normales
        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Enemy"),
            false
        );
    }
}
