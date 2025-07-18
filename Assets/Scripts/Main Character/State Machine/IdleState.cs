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
        ctx.animator.SetBool("isMoving", false);
        ctx.animator.SetFloat("moveY", 0);
        ctx.rb.linearVelocity = Vector2.zero;
    }

    public void HandleInput()
    {
        if (!ctx.canMove) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            sm.ChangeState(ctx.DashState);
            return;
        }

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
