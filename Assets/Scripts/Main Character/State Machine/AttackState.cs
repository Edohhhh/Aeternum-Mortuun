using UnityEngine;

public class AttackState : IPlayerState
{
    private PlayerController ctx;
    private StateMachine sm;
    private CombatSystem combat;

    private float stateTimer;
    private bool isComboWindowOpen;
    private bool attackBuffered;

    private Vector2 attackDir;
    private float currentSpeed;
    private float velocityDecay;

    private bool hasHitEnemy;

    public AttackState(PlayerController context, StateMachine stateMachine)
    {
        ctx = context;
        sm = stateMachine;
        combat = ctx.GetComponent<CombatSystem>();
    }

    public void Enter()
    {
        ctx.rb.linearVelocity = Vector2.zero;
        stateTimer = 0f;
        isComboWindowOpen = false;
        attackBuffered = false;
        hasHitEnemy = false;

        attackDir = combat.GetAttackDirection();
        combat.ExecuteAttackLogic(attackDir);

        // Configuración de movimiento (Recoil inicial)
        if (combat.recoilDuration > 0)
        {
            currentSpeed = combat.recoilDistance / combat.recoilDuration;
            velocityDecay = currentSpeed / combat.recoilDuration;
        }
        else
        {
            currentSpeed = 0;
            velocityDecay = 0;
        }

        // Animación
        if (ctx.animator != null)
        {
            ctx.animator.SetBool("isAttacking", true);
            ctx.animator.SetTrigger("attackTrigger");
            ctx.animator.SetFloat("attackX", attackDir.x);
            ctx.animator.SetFloat("attackY", attackDir.y);
        }
    }

    public void ApplyHitRecoil()
    {
        if (hasHitEnemy) return;

        hasHitEnemy = true;
        currentSpeed = combat.hitRecoilForce;
        velocityDecay = currentSpeed / combat.hitRecoilDuration;
    }

    public void HandleInput()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (stateTimer > combat.attackCooldown * 0.2f)
            {
                attackBuffered = true;
            }
        }

        if (Input.GetButtonDown("Jump") && ctx.dashCooldownTimer <= 0)
        {
            combat.ResetCombo();
            Vector2 dashDir = ctx.GetMoveInput().sqrMagnitude > 0 ? ctx.GetMoveInput() : attackDir;
            ctx.RequestedDashDir = dashDir;
            sm.ChangeState(ctx.DashState);
        }
    }

    public void LogicUpdate()
    {
        stateTimer += Time.deltaTime;

        if (stateTimer > combat.attackCooldown * 0.6f)
        {
            isComboWindowOpen = true;
        }

        // --- FINAL DEL ATAQUE ---
        if (stateTimer >= combat.attackCooldown)
        {
            // ✅ CAMBIO CLAVE: SIEMPRE preparamos el siguiente golpe del combo al terminar.
            // Si el jugador clickea dentro de 2 segundos, usará este nuevo índice.
            // Si pasan 2 segundos, el CombatSystem lo reseteará a 0 solo.
            combat.AdvanceCombo();

            if (attackBuffered && isComboWindowOpen)
            {
                // Si ya había clickeado, seguimos inmediatamente
                sm.ChangeState(ctx.AttackState);
            }
            else
            {
                // Si no, vamos a Idle. Pero el "AdvanceCombo" ya dejó listo el siguiente golpe.
                sm.ChangeState(ctx.IdleState);
            }
        }
    }

    public void PhysicsUpdate()
    {
        if (currentSpeed > 0)
        {
            Vector2 moveDir = hasHitEnemy ? -attackDir : attackDir;
            ctx.rb.linearVelocity = moveDir * currentSpeed;
            currentSpeed -= velocityDecay * Time.fixedDeltaTime;
        }
        else
        {
            ctx.rb.linearVelocity = Vector2.zero;
        }
    }

    public void Exit()
    {
        if (!attackBuffered && ctx.animator != null)
        {
            ctx.animator.SetBool("isAttacking", false);
        }
    }
}