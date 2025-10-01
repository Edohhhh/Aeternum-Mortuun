using UnityEngine;

public class GolemHeavyAttackState : State<EnemyInputs>
{
    private enum Phase { Windup, Jump, Impact }
    private Phase phase;

    private readonly GolemController golem;

    // F�sicas guardadas
    private RigidbodyConstraints2D savedConstraints;
    private bool savedSimulated;
    private float savedGravity;

    // control
    private float timer;
    private bool impacted;
    private bool jumpStarted;
    private bool finished; // evita doble salida

    // trayectoria
    private Vector2 jumpStart;
    private Vector2 jumpTarget;

    private float airTimeThisJump;
    private float arcThisJump;

    public GolemHeavyAttackState(GolemController g) { golem = g; }

    public override void Awake()
    {
        base.Awake();
        Debug.Log("Awake state: GolemHeavyAttackState");

        impacted = false;
        jumpStarted = false;
        finished = false;
        timer = 0f;
        phase = Phase.Windup;

        // Sellar cooldown al entrar al estado (�nico lugar)
        golem.MarkHeavyUsed();

        // Congelar y neutralizar gravedad para que no se mueva ni caiga
        if (golem.Body != null)
        {
            savedConstraints = golem.Body.constraints;
            savedSimulated = golem.Body.simulated;
            savedGravity = golem.Body.gravityScale;

            golem.Body.linearVelocity = Vector2.zero;
            golem.Body.gravityScale = 0f; // sin ca�da durante todo el heavy
            golem.Body.constraints = savedConstraints
                                        | RigidbodyConstraints2D.FreezePosition
                                        | RigidbodyConstraints2D.FreezeRotation;
        }

        // Disparo anim "Heavy"
        golem.Animator.ResetTrigger("Heavy");
        golem.Animator.SetTrigger("Heavy");

        // para recibir eventos
        golem.RegisterHeavyState(this);
    }

    public override void Execute()
    {
        switch (phase)
        {
            case Phase.Windup:
                if (golem.Body) golem.Body.linearVelocity = Vector2.zero;
                LookAtPlayer();
                break;

            case Phase.Jump:
                timer += Time.deltaTime;
                UpdateParabola();

                // Fallback por tiempo ante falta de evento de impacto
                if (!impacted && timer >= Mathf.Max(0.01f, golem.HeavyAirTime))
                    DoImpact();
                break;

            case Phase.Impact:
                // mantener pose/posici�n congeladas hasta el evento OnHeavyFinished
                if (golem.Body) golem.Body.linearVelocity = Vector2.zero;
                break;
        }
    }

    public override void Sleep()
    {
        // Restaurar f�sicas
        if (golem.Body != null)
        {
            golem.Body.constraints = savedConstraints | RigidbodyConstraints2D.FreezeRotation;
            golem.Body.simulated = savedSimulated;
            golem.Body.gravityScale = savedGravity;
            golem.Body.linearVelocity = Vector2.zero;
        }

        golem.RegisterHeavyState(null);
        base.Sleep();
    }

    // ================= Animation Events =================

    public void OnJumpStart()
    {
        if (phase != Phase.Windup || jumpStarted) return;
        jumpStarted = true;

        jumpStart = golem.Transform.position;

        // 1) Punto base = posici�n actual del player (o fallback)
        var p = golem.GetPlayer();
        Vector2 aim = p ? (Vector2)p.position : (Vector2)golem.Transform.position + Vector2.right;

        // 2) Si el player tiene Rigidbody2D, predecimos un poco hacia adelante
        if (p != null)
        {
            var prb = p.GetComponent<Rigidbody2D>();
            if (prb != null)
            {
                // Nota: usar linearVelocity (no velocity) para evitar el warning CS0618
                aim += prb.linearVelocity * golem.HeavyLeadTime;
            }

            // 3) Mezcla suave entre "posici�n actual" y "posici�n predicha"
            //    0 = cae en donde est� ahora; 1 = cae donde estar� seg�n la velocidad
            Vector2 currentPos = p.position;
            aim = Vector2.Lerp(currentPos, aim, golem.HeavyPredictBlend);
        }

        // 4) Clampeamos la distancia f�sica del salto (no m�s all� de lo que puede)
        Vector2 toAim = aim - jumpStart;
        float maxRange = golem.HeavyRange; // tu m�ximo f�sico de salto
        if (toAim.sqrMagnitude > maxRange * maxRange)
            aim = jumpStart + toAim.normalized * maxRange;

        jumpTarget = aim;

        // Liberamos SOLO la posici�n para animar el salto manualmente
        if (golem.Body != null)
        {
            golem.Body.constraints = RigidbodyConstraints2D.FreezeRotation;
            golem.Body.linearVelocity = Vector2.zero;
        }

        phase = Phase.Jump;
        timer = 0f;
    }
    //public void OnJumpStart()
    //{
    //    if (phase != Phase.Windup || jumpStarted) return;
    //    jumpStarted = true;

    //    jumpStart = golem.Transform.position;

    //    // Objetivo: posici�n del player acotada a heavyRange
    //    var p = golem.GetPlayer();
    //    Vector2 target = p ? (Vector2)p.position : (Vector2)golem.Transform.position + Vector2.right;
    //    Vector2 toTarget = target - jumpStart;

    //    float range = golem.HeavyRange;
    //    if (toTarget.magnitude > range)
    //        target = jumpStart + toTarget.normalized * range;

    //    jumpTarget = target;

    //    // Liberar SOLO la posici�n para animar el salto a mano
    //    if (golem.Body != null)
    //    {
    //        golem.Body.constraints = RigidbodyConstraints2D.FreezeRotation;
    //        golem.Body.linearVelocity = Vector2.zero;
    //    }

    //    phase = Phase.Jump;
    //    timer = 0f;
    //}

    public void OnImpact()
    {
        if (impacted) return;
        DoImpact();
    }

    public void OnFinished()
    {
        if (finished) return;
        // Si por timing llega sin haber marcado impacto, lo marcamos
        if (phase == Phase.Jump && !impacted) DoImpact();

        // Descongelar YA (no esperamos a Sleep)
        if (golem.Body != null)
        {
            golem.Body.constraints = savedConstraints | RigidbodyConstraints2D.FreezeRotation;
            golem.Body.gravityScale = savedGravity;
            golem.Body.linearVelocity = Vector2.zero;
        }

        // Volver a caminar
        if (golem.Animator)
        {
            golem.Animator.ResetTrigger("Heavy");
            golem.Animator.CrossFade("Walking", 0.01f); // o el clip base que uses
        }

        finished = true;
        golem.Transition(EnemyInputs.SeePlayer);
    }

    // ================= helpers =================


    private void UpdateParabola()
    {
        float t = Mathf.Clamp01(timer / Mathf.Max(0.01f, golem.HeavyAirTime));
        Vector2 pos = Vector2.Lerp(jumpStart, jumpTarget, t);
        float arc = golem.HeavyArcHeight * 4f * t * (1f - t); // par�bola simple

        golem.Transform.position = new Vector3(pos.x, pos.y + arc, golem.Transform.position.z);
        LookAtPlayer();
    }

    private void DoImpact()
    {
        impacted = true;
        phase = Phase.Impact;

        // congelar posici�n en el frame de impacto (pose del golpe)
        if (golem.Body)
        {
            golem.Body.linearVelocity = Vector2.zero;
            golem.Body.constraints = RigidbodyConstraints2D.FreezePosition
                                     | RigidbodyConstraints2D.FreezeRotation;
        }

        // da�o en �rea
        Vector2 c = golem.Transform.position;
        var hits = Physics2D.OverlapCircleAll(c, golem.HeavyDamageRadius, golem.HeavyPlayerMask);
        foreach (var h in hits)
        {
            var hp = h.GetComponent<PlayerHealth>() ?? h.GetComponentInParent<PlayerHealth>();
            if (hp != null) hp.TakeDamage(golem.HeavyDamage, c);
        }

        // FX opcionales aqu� (polvo, screenshake, etc.)
    }

    private void LookAtPlayer()
    {
        var p = golem.GetPlayer(); if (!p) return;
        Vector2 d = p.position - golem.Transform.position;
        var sr = golem.GetComponent<SpriteRenderer>();
        if (sr) sr.flipX = d.x < 0;
    }
}
