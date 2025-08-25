using System.Collections;
using UnityEngine;

public class UnderGroundAttackState : State<EnemyInputs>
{
    private readonly SkeletonController controller;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;
    private Animator anim;

    private RigidbodyConstraints2D originalConstraints;

    // Distancia lateral al reaparecer
    private const float APPEAR_OFFSET = 0.9f;

    private const float SNAPSHOT_LEAD = 0.3f;

    // Tiempo oculto bajo tierra
    private const float HIDE_SECONDS = 3f;

    // Ventana de gracia sin colisiones para evitar daño instantáneo
    private const float NO_COLLISION_GRACE = 0.25f;

    private bool burrowDone;

    // Posición "con delay" del jugador (snapshot)
    private Vector3 delayedPlayerPos;

    public UnderGroundAttackState(SkeletonController controller, float _buryDur, float _emergeDur)
    {
        this.controller = controller;
    }

    public override void Awake()
    {
        base.Awake();

        rb = controller.GetComponent<Rigidbody2D>();
        sr = controller.GetComponent<SpriteRenderer>();
        col = controller.GetComponent<Collider2D>();
        anim = controller.GetComponent<Animator>();

        controller.RegisterUnderGroundState(this);

        if (rb != null)
        {
            originalConstraints = rb.constraints;
            rb.linearVelocity = Vector2.zero; // <- corregido para 2D
            rb.constraints = originalConstraints
                             | RigidbodyConstraints2D.FreezePosition
                             | RigidbodyConstraints2D.FreezeRotation;
        }

        if (sr != null) sr.enabled = true;
        if (col != null) col.enabled = true;

        if (anim != null)
        {
            anim.ResetTrigger("Burrow");
            anim.SetTrigger("Burrow");
        }

        burrowDone = false;
    }

    // Llamado desde Animation Event al final del clip "Burrow"
    public void OnBurrowAnimFinished()
    {
        if (burrowDone) return;
        burrowDone = true;

        // Tomamos un "snapshot" de la posición del jugador AHORA
        var player = controller.GetPlayer();
        delayedPlayerPos = player != null ? player.position : controller.transform.position;

        controller.StartCoroutine(BurrowSequence());
    }

    private IEnumerator BurrowSequence()
    {
        if (sr != null) sr.enabled = false;
        if (col != null) col.enabled = false;

        // Esperar la parte larga del ocultamiento
        float firstWait = Mathf.Max(0f, HIDE_SECONDS - SNAPSHOT_LEAD);
        yield return new WaitForSecondsRealtime(firstWait);

        // Tomar la posición del jugador "SNAPSHOT_LEAD" antes de reaparecer
        var player = controller.GetPlayer();
        delayedPlayerPos = player != null ? player.position : controller.transform.position;

        // Esperar el “anticipo” restante
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, SNAPSHOT_LEAD));

        // TP usando la posición recién tomada (no tan vieja)
        float side = (controller.transform.position.x <= delayedPlayerPos.x) ? -1f : 1f;
        Vector3 target = new Vector3(delayedPlayerPos.x + side * APPEAR_OFFSET,
                                     delayedPlayerPos.y,
                                     controller.transform.position.z);
        controller.transform.position = target;

        if (sr != null) sr.enabled = true;

        if (rb != null)
            rb.constraints = originalConstraints | RigidbodyConstraints2D.FreezeRotation;

        controller.StartCoroutine(ReenableColliderAfter(NO_COLLISION_GRACE));

        controller.Transition(EnemyInputs.Spawn);
    }

    private IEnumerator ReenableColliderAfter(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        if (col != null) col.enabled = true;
    }

    public override void Execute() { /* Manejado por la coroutine */ }

    public override void Sleep()
    {
        base.Sleep();
        controller.RegisterUnderGroundState(null);

        if (rb != null)
            rb.constraints = originalConstraints | RigidbodyConstraints2D.FreezeRotation;

        if (sr != null && !sr.enabled) sr.enabled = true;
        if (col != null && !col.enabled) col.enabled = true;
    }
}
//using System.Collections;
//using UnityEngine;

//public class UnderGroundAttackState : State<EnemyInputs>
//{
//    private readonly SkeletonController controller;

//    private Rigidbody2D rb;
//    private SpriteRenderer sr;
//    private Collider2D col;
//    private Animator anim;

//    private RigidbodyConstraints2D originalConstraints;

//    private const float APPEAR_OFFSET = 0.9f;
//    private bool burrowDone;

//    public UnderGroundAttackState(SkeletonController controller, float _buryDur, float _emergeDur)
//    {
//        this.controller = controller;
//    }

//    public override void Awake()
//    {
//        base.Awake();

//        rb = controller.GetComponent<Rigidbody2D>();
//        sr = controller.GetComponent<SpriteRenderer>();
//        col = controller.GetComponent<Collider2D>();
//        anim = controller.GetComponent<Animator>();

//        // Solo necesitamos recibir el evento de Burrow
//        controller.RegisterUnderGroundState(this);

//        if (rb != null)
//        {
//            originalConstraints = rb.constraints;
//            rb.linearVelocity = Vector2.zero;
//            rb.constraints = originalConstraints
//                             | RigidbodyConstraints2D.FreezePosition
//                             | RigidbodyConstraints2D.FreezeRotation;
//        }

//        if (sr != null) sr.enabled = true;
//        if (col != null) col.enabled = true;

//        if (anim != null)
//        {
//            anim.ResetTrigger("Burrow");
//            anim.SetTrigger("Burrow");
//        }

//        burrowDone = false;
//    }

//    // ---------- llamado por el controller desde Animation Event del clip "Burrow" ----------
//    public void OnBurrowAnimFinished()
//    {
//        if (burrowDone) return;
//        burrowDone = true;
//        controller.StartCoroutine(BurrowSequence());
//    }

//    private IEnumerator BurrowSequence()
//    {
//        // 1) ocultar 3s
//        if (sr != null) sr.enabled = false;
//        if (col != null) col.enabled = false;

//        yield return new WaitForSecondsRealtime(3f);

//        // 2) TP al lado exacto del player
//        var player = controller.GetPlayer();
//        if (player != null)
//        {
//            float side = (controller.transform.position.x <= player.position.x) ? -1f : 1f;
//            Vector3 pos = new Vector3(player.position.x + side * APPEAR_OFFSET,
//                                      player.position.y,
//                                      controller.transform.position.z);
//            controller.transform.position = pos;
//        }

//        // 3) reactivar sprite+collider (así el siguiente estado "Spawn" ya lo ve encendido)
//        if (sr != null) sr.enabled = true;
//        if (col != null) col.enabled = true;

//        // 4) restaurar físicas (ya no hace falta seguir congelado)
//        if (rb != null)
//            rb.constraints = originalConstraints | RigidbodyConstraints2D.FreezeRotation;

//        // 5) pasar al estado Spawn
//        controller.Transition(EnemyInputs.Spawn);
//    }

//    public override void Execute()
//    {
//        // Nada: todo se maneja por evento+coroutine y transición explícita
//    }

//    public override void Sleep()
//    {
//        base.Sleep();
//        controller.RegisterUnderGroundState(null);

//        if (rb != null)
//            rb.constraints = originalConstraints | RigidbodyConstraints2D.FreezeRotation;

//        if (sr != null && !sr.enabled) sr.enabled = true;
//        if (col != null && !col.enabled) col.enabled = true;
//    }
//}