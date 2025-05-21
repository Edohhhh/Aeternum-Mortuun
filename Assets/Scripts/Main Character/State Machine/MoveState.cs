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
        ctx.animator.SetBool("isMoving", true);
    }
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
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (input == Vector2.zero && velocity.magnitude <= 0.05f)
        {
            velocity = Vector2.zero;
            sm.ChangeState(ctx.IdleState);
        }
    }

    public void LogicUpdate()
    {
        if (input != Vector2.zero)
        {
            ctx.animator.SetFloat("moveY", input.y); // importante para elegir la animación
            velocity = Vector2.MoveTowards(velocity, input * ctx.moveSpeed,
                ctx.acceleration * Time.deltaTime);
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
        ctx.rb.MovePosition(ctx.rb.position + velocity * Time.fixedDeltaTime);
    }

    public void Exit()
    {
        ctx.animator.SetBool("isMoving", false);
        ctx.animator.SetFloat("moveY", 0); // opcional para evitar mantener valor
    }
}
