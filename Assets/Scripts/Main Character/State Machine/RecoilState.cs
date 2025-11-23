using UnityEngine;

public class RecoilState : IPlayerState
{
    private PlayerController ctx;
    private StateMachine sm;
    private float timer;
    private float duration;
    private Vector2 dir;
    private float distance;

    public RecoilState(PlayerController context, StateMachine stateMachine)
    {
        ctx = context;
        sm = stateMachine;
    }

    public void Enter()
    {
        var combat = ctx.GetComponent<CombatSystem>();
        duration = combat.recoilDuration;
        distance = combat.recoilDistance;
        dir = combat.LastAttackDir;

        ctx.rb.linearVelocity = Vector2.zero;

        // 🚫 Bloquear movimiento mientras dure el recoil
        ctx.canMove = false;
        timer = 0f;
    }

    public void HandleInput()
    {
        // Ignorar input durante recoil
    }

    public void LogicUpdate()
    {
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            ctx.canMove = true; // ✅ se libera control
            sm.ChangeState(ctx.IdleState);
        }
    }

    public void PhysicsUpdate()
    {
        // Movimiento de micro-dash en la dirección del ataque
        float speed = distance / duration;
        ctx.rb.MovePosition(ctx.rb.position + (-dir) * speed * Time.fixedDeltaTime);



    }

    public void Exit()
    {
        ctx.rb.linearVelocity = Vector2.zero;
        ctx.IsDashing = false; // seguridad extra
    }
}
