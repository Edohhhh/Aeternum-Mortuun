//using UnityEngine;

//public class GolemHeavyAttackState : State<EnemyInputs>
//{
//    private enum Phase { Windup, Jump, Impact, Stun }
//    private Phase phase;

//    private readonly GolemController golem;
//    private RigidbodyConstraints2D savedConstraints;

//    // timing
//    private float timer;
//    private Vector2 jumpStart, jumpTarget;
//    private bool impacted;

//    // cache
//    private Transform tr;
//    private Rigidbody2D body;
//    private Animator anim;
//    private SpriteRenderer sr;

//    public GolemHeavyAttackState(GolemController g) { golem = g; }

//    public override void Awake()
//    {
//        base.Awake();
//        tr = golem.Transform;
//        body = golem.Body;
//        anim = golem.Animator;
//        sr = golem.GetComponent<SpriteRenderer>();

//        // marcar cooldown (contador interno se resetea)
//        golem.MarkHeavyUsed();

//        // frenar movimiento
//        if (body)
//        {
//            savedConstraints = body.constraints;
//            body.linearVelocity = Vector2.zero;
//            body.constraints = savedConstraints
//                | RigidbodyConstraints2D.FreezePosition
//                | RigidbodyConstraints2D.FreezeRotation;
//        }

//        // Dispara la anim del heavy (clip con 3 events: JumpStart, Impact, Finished)
//        anim.ResetTrigger("Heavy");
//        anim.SetTrigger("Heavy");

//        // setup fase
//        phase = Phase.Windup;
//        timer = 0f;
//        impacted = false;

//        // registrar para recibir eventos
//        golem.RegisterHeavyState(this);
//    }

//    public override void Execute()
//    {
//        timer += Time.deltaTime;

//        switch (phase)
//        {
//            case Phase.Windup:
//                FacePlayer();
//                break;

//            case Phase.Jump:
//                // Duración fija del aire
//                float t = Mathf.Clamp01(timer / golem.HeavyAirTime);
//                // interpolación + arquito simple
//                Vector2 pos = Vector2.Lerp(jumpStart, jumpTarget, t);
//                float arc = golem.HeavyArcHeight * 4f * t * (1f - t);
//                tr.position = new Vector2(pos.x, pos.y + arc);

//                if (t >= 1f && !impacted)
//                    DoImpact();
//                break;

//            case Phase.Impact:
//                // Pequeña ventana por si querés un “golpe” corto antes del stun
//                if (timer >= 0.1f) StartStun();
//                break;

//            case Phase.Stun:
//                if (timer >= golem.HeavyStunTime)
//                    golem.Transition(EnemyInputs.SeePlayer);
//                break;
//        }
//    }

//    public override void Sleep()
//    {
//        // restaurar movimiento
//        if (body) body.constraints = savedConstraints;

//        golem.RegisterHeavyState(null);
//        base.Sleep();
//    }

//    // ---------- Animation Events ----------
//    public void OnJumpStart()
//    {
//        if (phase != Phase.Windup) return;

//        // snapshot de la posición del player
//        var p = golem.GetPlayer();
//        jumpStart = tr.position;
//        jumpTarget = p ? (Vector2)p.position : jumpStart; // si no hay player, se queda
//        timer = 0f;
//        phase = Phase.Jump;
//    }

//    public void OnImpact()
//    {
//        if (phase == Phase.Jump && !impacted)
//            DoImpact();
//    }

//    public void OnFinished()
//    {
//        if (phase == Phase.Impact) StartStun();
//    }
//    // --------------------------------------

//    private void DoImpact()
//    {
//        impacted = true;
//        timer = 0f;
//        phase = Phase.Impact;

//        // Daño en área
//        Vector2 center = tr.position;
//        var hits = Physics2D.OverlapCircleAll(center, golem.HeavyDamageRadius, golem.BeamPlayerMask);
//        foreach (var h in hits)
//        {
//            var hp = h.GetComponent<PlayerHealth>() ?? h.GetComponentInParent<PlayerHealth>();
//            if (hp) hp.TakeDamage(golem.HeavyDamage, center);
//        }
//    }

//    private void StartStun()
//    {
//        phase = Phase.Stun;
//        timer = 0f;
//    }

//    private void FacePlayer()
//    {
//        var p = golem.GetPlayer();
//        if (!p) return;
//        Vector2 dir = p.position - tr.position;
//        if (sr) sr.flipX = dir.x < 0f;
//    }
//}
