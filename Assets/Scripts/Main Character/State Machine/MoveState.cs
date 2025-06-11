using UnityEngine;

public class MoveState : IPlayerState
{
    private PlayerController ctx;
    private StateMachine sm;
    private Vector2 input;
    private Vector2 velocity;

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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            sm.ChangeState(ctx.DashState);
            return;
        }

        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (input == Vector2.zero && velocity.magnitude <= 0.05f)
        {
            velocity = Vector2.zero;
            sm.ChangeState(ctx.IdleState);
        }
    }

    public void LogicUpdate()
    {
        if (!ctx.canMove)
        {
            velocity = Vector2.zero;
            return;
        }

        if (input != Vector2.zero)
        {
            ctx.animator.SetFloat("moveY", input.y);
            velocity = Vector2.MoveTowards(velocity, input * ctx.moveSpeed, ctx.acceleration * Time.deltaTime);
        }
        else
        {
            float decelAmount = ctx.deceleration * Time.deltaTime;
            if (velocity.magnitude <= decelAmount)
            {
                velocity = Vector2.zero;
            }
            else
            {
                velocity = velocity.normalized * (velocity.magnitude - decelAmount);
            }
        }
    }

    public void PhysicsUpdate()
    {
        if (!ctx.canMove)
        {
            // 🔥 Durante recoil o bloqueo: no toques nada
            return;
        }

        // 🔥 Movimiento normal
        ctx.rb.MovePosition(ctx.rb.position + velocity * Time.fixedDeltaTime);
    }

    public void Exit()
    {
        if (ctx.animator != null)
            ctx.animator.SetBool("isMoving", false);
        ctx.animator.SetFloat("moveY", 0);
    }
}
