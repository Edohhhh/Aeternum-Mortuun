using UnityEngine;
using System.Collections;

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
        // 🔥 Calcula la dirección del dash solo al iniciar
        dashDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (dashDir == Vector2.zero) dashDir = Vector2.right;

        // 🔥 Activa invulnerabilidad
        ctx.isInvulnerable = true;

        // 🔥 Configura el timer
        dashTimer = ctx.dashDuration;

        // 🔥 Desactiva hitbox si corresponde
        if (ctx.GetComponent<Collider2D>() != null)
        {
            ctx.hitbox.enabled = false;
        }

        // 🔥 Animación (opcional)
        ctx.GetComponent<Animator>()?.SetBool("isDashing", true);
    }

    public void HandleInput() { }
    public void LogicUpdate() { }

    public void PhysicsUpdate()
    {
        // 🔥 Aplica el movimiento del dash
        ctx.rb.linearVelocity = dashDir * ctx.dashSpeed;

        // 🔥 Actualiza el temporizador
        dashTimer -= Time.fixedDeltaTime;
        if (dashTimer <= 0f)
        {
            // 🔥 Termina el dash
            ctx.rb.linearVelocity = Vector2.zero;
            ctx.isInvulnerable = false;

            // 🔥 Reactiva hitbox si corresponde
            if (ctx.GetComponent<Collider2D>() != null)
            {
                ctx.hitbox.enabled = true;
            }

            // 🔥 Apaga animación (opcional)
            ctx.GetComponent<Animator>()?.SetBool("isDashing", false);

            // 🔥 Cambia al estado Idle
            sm.ChangeState(ctx.IdleState);
        }
    }

    public void Exit()
    {
        // 🔥 Asegura que la animación de dash se apague siempre
        if (ctx.animator != null)
            ctx.animator.SetBool("isDashing", false);

        // 🔥 Reactiva el hitbox y la vulnerabilidad
        if (ctx.hitbox != null) ctx.hitbox.enabled = true;
        ctx.isInvulnerable = false;

        // 🔥 Reactiva el movimiento
        ctx.canMove = true;
        ctx.isInvulnerable = false;
    }
}
