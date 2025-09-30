using UnityEngine;

public class MoveState : IPlayerState
{
    private PlayerController ctx;
    private StateMachine sm;
    private Vector2 input;

    public MoveState(PlayerController context, StateMachine stateMachine)
    {
        ctx = context;
        sm = stateMachine;
    }

    public void Enter()
    {
        if (ctx.animator != null)
            ctx.animator.SetBool("isMoving", true);
    }

    public void HandleInput()
    {
        if (!ctx.canMove) return;

        // 🔸 Dash se maneja SOLO en PlayerController.Update()

        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (input == Vector2.zero)
            sm.ChangeState(ctx.IdleState);
    }

    public void LogicUpdate()
    {
        if (!ctx.canMove) return;

        if (ctx.animator != null)
            ctx.animator.SetFloat("moveY", input.y);
    }

    public void PhysicsUpdate()
    {
        if (!ctx.canMove) return;

        ctx.rb.MovePosition(ctx.rb.position + input * ctx.moveSpeed * Time.fixedDeltaTime);
    }

    public void Exit()
    {
        if (ctx.animator != null)
        {
            ctx.animator.SetBool("isMoving", false);
            ctx.animator.SetFloat("moveY", 0);
        }
    }
}
