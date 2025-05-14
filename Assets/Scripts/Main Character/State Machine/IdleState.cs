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

    public void Enter() { }
    public void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            sm.ChangeState(ctx.DashState);
            return;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)
            || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            sm.ChangeState(ctx.AttackState);
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