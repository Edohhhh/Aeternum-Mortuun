using UnityEngine;
using System.Collections;

public class DashState : IPlayerState
{
    private PlayerController ctx;
    private StateMachine sm;
    private Vector2 dashDir;

    public DashState(PlayerController context, StateMachine stateMachine)
    {
        ctx = context;
        sm = stateMachine;
    }

    public void Enter()
    {
        dashDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (dashDir == Vector2.zero) dashDir = Vector2.right;

        if (ctx.hitbox != null) ctx.hitbox.enabled = false;

        ctx.animator.SetBool("isDashing", true); // ← activar animación
        ctx.StartCoroutine(DashRoutine());
    }


    public void HandleInput() { }
    public void LogicUpdate() { }
    public void PhysicsUpdate() { }
    public void Exit() { }

    private IEnumerator DashRoutine()
    {
        // Activar animación de dash
        ctx.animator.SetBool("isDashing", true);

        // Desactivar hitbox temporalmente
        if (ctx.hitbox != null)
            ctx.hitbox.enabled = false;

        // Fase principal del dash
        float elapsed = 0f;
        while (elapsed < ctx.dashDuration)
        {
            ctx.rb.linearVelocity = dashDir * ctx.dashSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Fase de deslizamiento post-dash
        float slideElapsed = 0f;
        while (slideElapsed < ctx.dashSlideDuration)
        {
            float t = slideElapsed / ctx.dashSlideDuration;
            ctx.rb.linearVelocity = dashDir * ctx.dashSpeed * (1f - t);
            slideElapsed += Time.deltaTime;
            yield return null;
        }

        // Finalizar movimiento
        ctx.rb.linearVelocity = Vector2.zero;

        // Reactivar hitbox
        if (ctx.hitbox != null)
            ctx.hitbox.enabled = true;

        // Desactivar animación de dash
        ctx.animator.SetBool("isDashing", false);

        // Cambiar a estado Idle
        sm.ChangeState(ctx.IdleState);

        // Esperar cooldown antes de permitir otro dash (si lo manejás en otro lado podés quitar esto)
        yield return new WaitForSeconds(ctx.dashCooldown);
    }

}