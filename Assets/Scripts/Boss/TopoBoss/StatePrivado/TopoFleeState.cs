using UnityEngine;

public class TopoFleeState : State<EnemyInputs>
{
    private readonly TopoController ctrl;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private RigidbodyConstraints2D savedConstraints;

    private bool running = false;
    private bool finished = false;

    private Transform fleeTarget; // waypoint destino

    public TopoFleeState(TopoController c) { ctrl = c; }

    public override void Awake()
    {
        base.Awake();

        rb = ctrl.GetComponent<Rigidbody2D>();
        anim = ctrl.GetComponent<Animator>();
        sr = ctrl.GetComponent<SpriteRenderer>();

        if (sr) sr.enabled = true;
        var col = ctrl.GetComponent<Collider2D>();
        if (col) col.enabled = true;

        // permitir movimiento
        if (rb)
        {
            savedConstraints = rb.constraints;
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        ctrl.RegisterFleeState(this);

        // ?? Elegir un waypoint aleatorio distinto al último
        fleeTarget = ctrl.GetRandomWaypointNotLast();
        if (fleeTarget == null)
        {
            Debug.LogWarning("[TopoFleeState] No hay waypoints definidos.");
        }

        // Disparar la animación de huida
        if (anim)
        {
            anim.ResetTrigger("Flee");
            anim.SetTrigger("Flee");
        }

        running = true;
        finished = false;
    }

    public override void Execute()
    {
        if (!running || finished || fleeTarget == null) return;

        // Movimiento hacia el waypoint
        Vector2 pos = ctrl.transform.position;
        Vector2 targetPos = fleeTarget.position;
        Vector2 dir = (targetPos - pos).normalized;

        ctrl.transform.position += (Vector3)(dir * ctrl.FleeSpeed * Time.deltaTime);

        // Opcional: si está muy cerca, consideramos que “llegó”
        if (Vector2.Distance(pos, targetPos) < 0.1f)
        {
            running = false;
            OnFleeAnimEnd(); // forzar el fin anticipado
        }
    }

    // Animation Event al final del clip Flee
    public void OnFleeAnimEnd()
    {
        if (finished) return;
        finished = true;
        running = false;

        if (anim) anim.SetTrigger("Burrow"); // meterse
        ctrl.Transition(EnemyInputs.SeePlayer);
    }

    public override void Sleep()
    {
        if (rb) rb.constraints = savedConstraints;
        base.Sleep();
    }
}


//using UnityEngine;

//public class TopoFleeState : State<EnemyInputs>
//{
//    private readonly TopoController ctrl;

//    private Rigidbody2D rb;
//    private Animator anim;
//    private SpriteRenderer sr;
//    private RigidbodyConstraints2D savedConstraints;

//    private bool running = false;
//    private bool finished = false;

//    public TopoFleeState(TopoController c) { ctrl = c; }

//    public override void Awake()
//    {
//        base.Awake();

//        rb = ctrl.GetComponent<Rigidbody2D>();
//        anim = ctrl.GetComponent<Animator>();
//        sr = ctrl.GetComponent<SpriteRenderer>();

//        if (sr) sr.enabled = true;
//        var col = ctrl.GetComponent<Collider2D>();
//        if (col) col.enabled = true;

//        // mover por transform (dejamos solo la rotación congelada)
//        if (rb)
//        {
//            savedConstraints = rb.constraints;
//            rb.linearVelocity = Vector2.zero;
//            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
//        }

//        ctrl.RegisterFleeState(this);

//        // Disparamos LA animación de huida (UNA sola anim)
//        if (anim)
//        {
//            anim.ResetTrigger("Flee");
//            anim.SetTrigger("Flee");
//        }

//        running = true;   // corre mientras el clip Flee se reproduce
//        finished = false;
//    }

//    public override void Execute()
//    {
//        if (!running || finished) return;

//        // Correr alejándose del jugador (mientras dura el clip Flee)
//        Transform p = ctrl.GetPlayer();
//        Vector2 dir = Vector2.right;

//        if (p)
//        {
//            Vector2 away = (Vector2)ctrl.transform.position - (Vector2)p.position;
//            dir = away.sqrMagnitude > 0.0001f ? away.normalized : Vector2.right;
//        }

//        ctrl.transform.position += (Vector3)(dir * ctrl.FleeSpeed * Time.deltaTime);
//    }

//    // Llamado por Animation Event al final del CLIP "Flee"
//    public void OnFleeAnimEnd()
//    {
//        if (finished) return;
//        finished = true;
//        running = false;

//        // 1) Enterrarse con tu animación existente
//        if (anim) anim.SetTrigger("Burrow");

//        // 2) Volver al ciclo normal (Idle). El enterrarse y el TP
//        // los siguen manejando tus Animation Events ya implementados.
//        ctrl.Transition(EnemyInputs.SeePlayer);
//    }

//    public override void Sleep()
//    {
//        if (rb) rb.constraints = savedConstraints;
//        base.Sleep();
//    }
//}
