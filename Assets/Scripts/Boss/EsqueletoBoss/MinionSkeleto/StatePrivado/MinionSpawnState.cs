using UnityEngine;

public class MinionSpawnState : State<EnemyInputs>
{
    private readonly MinionController controller;
    private bool finished;

    public MinionSpawnState(MinionController ctrl)
    {
        controller = ctrl;
    }

    public override void Awake()
    {
        base.Awake();
        finished = false;

        // El controller queda listo para recibir el evento del clip "Spawn"
        controller.RegisterSpawnState(this);

        // YA NO disparamos el trigger aquí (lo reproduce el Animator al entrar desde Entry)
        // var anim = controller.GetComponent<Animator>();
        // anim.ResetTrigger("Spawn");
        // anim.SetTrigger("Spawn");

        // Quieto durante el spawn
        var rb = controller.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.constraints |= RigidbodyConstraints2D.FreezePosition;
            rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
        }
    }

    /// <summary>
    /// Llamado por MinionController.OnMinionSpawnFinished() desde el Animation Event.
    /// </summary>
    public void NotifySpawnFinished()
    {
        // Forzamos al Animator a salir YA del clip Spawn
        var anim = controller.GetComponent<Animator>();
        if (anim != null)
        {
            anim.ResetTrigger("Spawn");           // por si existe el parámetro
            anim.CrossFade("Walking", 0.01f);     // o anim.Play("Walking", 0, 0f);
        }

        finished = true;
    }

    public override void Execute()
    {
        if (!finished) return;

        // Liberar la posición y mantener Z congelada
        var rb = controller.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.constraints &= ~RigidbodyConstraints2D.FreezePosition;
            rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
        }

        // Entrar a follow
        controller.Transition(EnemyInputs.SeePlayer);
    }

    public override void Sleep()
    {
        base.Sleep();
        controller.RegisterSpawnState(null);

        // Seguridad extra: limpiar el trigger si salimos sin evento
        var anim = controller.GetComponent<Animator>();
        if (anim != null) anim.ResetTrigger("Spawn");
    }
}

//public class MinionSpawnState : State<EnemyInputs>
//{
//    readonly MinionController controller;
//    readonly float duration;
//    float timer;
//    bool transitioned;

//    public MinionSpawnState(MinionController ctrl, float spawnAnimDuration)
//    {
//        controller = ctrl;
//        duration = spawnAnimDuration;
//    }

//    public override void Awake()
//    {
//        base.Awake();
//        timer = 0f;
//        transitioned = false;

//        // 1) Disparar anim de spawn
//        controller.GetComponent<Animator>()?.SetTrigger("Spawn");

//        // 2) Congelar posición para que no lo empuje ni se mueva
//        var rb = controller.GetComponent<Rigidbody2D>();
//        if (rb != null)
//            rb.constraints |= RigidbodyConstraints2D.FreezePosition;
//    }

//    public override void Execute()
//    {
//        if (transitioned) return;

//        timer += Time.deltaTime;
//        if (timer < duration) return;

//        transitioned = true;

//        // 3) Liberar la posición (seguimos congelando Z)
//        var rb = controller.GetComponent<Rigidbody2D>();
//        if (rb != null)
//        {
//            rb.constraints &= ~RigidbodyConstraints2D.FreezePosition;
//            rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
//        }

//        // 4) Pasar a seguir al jugador
//        controller.Transition(EnemyInputs.SeePlayer);
//    }

//    public override void Sleep()
//    {
//        base.Sleep();
//        // no hay nada más que limpiar
//    }
//}