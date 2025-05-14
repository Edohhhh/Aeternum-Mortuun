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
        ctx.StartCoroutine(DashRoutine());
    }
    public void HandleInput() { }
    public void LogicUpdate() { }
    public void PhysicsUpdate() { }
    public void Exit() { }

    private IEnumerator DashRoutine()
    {
        // Main dash
        float elapsed = 0f;
        while (elapsed < ctx.dashDuration)
        {
            ctx.rb.linearVelocity = dashDir * ctx.dashSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }
        // Post-dash smoothing slide
        float slideElapsed = 0f;
        while (slideElapsed < ctx.dashSlideDuration)
        {
            float t = slideElapsed / ctx.dashSlideDuration;
            ctx.rb.linearVelocity = dashDir * ctx.dashSpeed * (1f - t);
            slideElapsed += Time.deltaTime;
            yield return null;
        }
        // End movement
        ctx.rb.linearVelocity = Vector2.zero;
        if (ctx.hitbox != null) ctx.hitbox.enabled = true;
        sm.ChangeState(ctx.IdleState);
        yield return new WaitForSeconds(ctx.dashCooldown);
    }
}