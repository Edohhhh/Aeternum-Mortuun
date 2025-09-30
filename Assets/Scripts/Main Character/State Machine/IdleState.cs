using UnityEngine;

public class IdleState : IPlayerState
{
    private PlayerController ctx;
    private StateMachine sm;

    public IdleState(PlayerController context, StateMachine stateMachine)
    {
        ctx = context;
        sm = stateMachine;
    }

    public void Enter()
    {
        if (ctx.animator != null)
        {
            ctx.animator.SetBool("isMoving", false);
            ctx.animator.SetFloat("moveY", 0);
        }
        ctx.rb.linearVelocity = Vector2.zero;
    }

    public void HandleInput()
    {
        if (!ctx.canMove) return;

        // 🔸 Dash se maneja SOLO en PlayerController.Update()

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (input != Vector2.zero)
        {
            sm.ChangeState(ctx.MoveState);
        }
    }

    public void LogicUpdate() { }
    public void PhysicsUpdate() { }
    public void Exit() { }
}
