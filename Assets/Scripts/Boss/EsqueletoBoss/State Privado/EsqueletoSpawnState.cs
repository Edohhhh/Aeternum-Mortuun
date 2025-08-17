using UnityEngine;

public class EsqueletoSpawnState : State<EnemyInputs>
{
    private readonly SkeletonController controller;
    private bool finished;

    public EsqueletoSpawnState(SkeletonController ctrl, float _ignoredDuration = 0f)
    {
        controller = ctrl;
    }

    public override void Awake()
    {
        base.Awake();
        finished = false;

        // Nos registramos para que el Animation Event nos avise
        controller.RegisterSpawnState(this);

        // Disparo del clip Spawn
        //var anim = controller.GetComponent<Animator>();
        //anim.ResetTrigger("Spawn");
        //anim.SetTrigger("Spawn");

        // (Opcional) Congelar la posición durante la animación:
        var rb = controller.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.constraints |= RigidbodyConstraints2D.FreezePosition;   // se libera al salir
            rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
        }
    }

    public void NotifySpawnFinished()
    {
        var anim = controller.GetComponent<Animator>();
        if (anim != null)
        {
            anim.ResetTrigger("Spawn");
            anim.CrossFade("Walking", 0.01f);  // o anim.Play("Walking", 0, 0);
        }
        finished = true;
    }

    public override void Execute()
    {
        if (!finished) return;

        // Liberar congelamiento antes de salir
        var rb = controller.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.constraints &= ~RigidbodyConstraints2D.FreezePosition;  // solo posición
            rb.constraints |= RigidbodyConstraints2D.FreezeRotation;   // mantener rotación congelada
        }

        // Pasar a seguir al jugador
        controller.Transition(EnemyInputs.SeePlayer);

        // Programar el spawn de minions con delay “de paseo”
        controller.Invoke(nameof(SkeletonController.DoSpawnMinions),
                          controller.initialSpawnMinionDelay);
    }

    public override void Sleep()
    {
        base.Sleep();
        controller.RegisterSpawnState(null);

        //Por si salimos sin Event, aseguro limpiar el trigger
        var anim = controller.GetComponent<Animator>();
        if (anim != null) anim.ResetTrigger("Spawn");
    }
}
//public class EsqueletoSpawnState : State<EnemyInputs>
//{
//    private readonly SkeletonController controller;
//    private readonly float duration;
//    private float timer;
//    private bool hasTransitioned;

//    public EsqueletoSpawnState(SkeletonController ctrl, float spawnAnimDuration)
//    {
//        controller = ctrl;
//        duration = spawnAnimDuration;
//    }

//    public override void Awake()
//    {
//        base.Awake();
//        timer = 0f;
//        hasTransitioned = false;
//        controller.GetComponent<Animator>()?.SetTrigger("Spawn");
//    }

//    public override void Execute()
//    {
//        if (hasTransitioned) return;

//        timer += Time.deltaTime;
//        if (timer < duration) return;

//        hasTransitioned = true;
//        controller.Transition(EnemyInputs.SeePlayer);
//        //  SOLO invocamos minions tras un pequeño paseo
//        controller.Invoke(
//            nameof(SkeletonController.DoSpawnMinions),
//            controller.initialSpawnMinionDelay
//            );
//    }
//}
