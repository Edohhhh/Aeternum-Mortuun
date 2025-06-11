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
        ctx.animator.SetBool("isMoving", false); // cuando esté en Idle
    }
    public void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            sm.ChangeState(ctx.DashState);
            return;
        }
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A)
            || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            sm.ChangeState(ctx.MoveState);
        }
    }
    public void LogicUpdate() { }
    public void PhysicsUpdate() { }
    public void Exit() { }
}