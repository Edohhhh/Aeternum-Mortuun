using UnityEngine;

public class MimicoAwakenState : State<EnemyInputs>
{
    private readonly MimicoController ctrl;

    private Rigidbody2D rb;
    private Animator anim;
    private RigidbodyConstraints2D saved;
    private bool finished;
    private bool frozen;

    public MimicoAwakenState(MimicoController c) { ctrl = c; }

    public override void Awake()
    {
        base.Awake();
        finished = false;

        rb = ctrl.GetComponent<Rigidbody2D>();
        anim = ctrl.Animator;

        // Congelar durante el “despertar”
        if (rb)
        {
            saved = rb.constraints;
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
            frozen = true;
        }

        // Reenviar eventos a este estado
        ctrl.RegisterAwakenState(this);

        ctrl.SetBossCollidersEnabled(false);

        // Asegurar icono oculto durante Awaken
        ctrl.SetDormantIconVisible(false);

        // Disparar la anim de despertar
        if (anim)
        {
            anim.SetBool("isWalking", false);
            anim.ResetTrigger("Melee");
            anim.ResetTrigger("Die");
            anim.ResetTrigger("Awaken");
            anim.SetTrigger("Awaken"); 
        }
    }


    // Llamado desde Animation Event al terminar el clip “Awaken”
    public void OnAwakenFinished()
    {
        finished = true;

        if (ctrl != null && ctrl.colliderDummy != null)
            Object.Destroy(ctrl.colliderDummy);

        //ctrl.SetDummyColliderActive(false);

        ctrl.SetBossCollidersEnabled(true);

        // (opcional) destruir icono definitivamente
        ctrl.DestroyDormantIcon();
    }

    public override void Execute()
    {
        if (!finished) return;

        // Liberar físicas antes de salir
        if (frozen && rb)
        {
            rb.constraints = saved;
            frozen = false;
        }

        // Ir a seguir al jugador
        ctrl.Transition(EnemyInputs.SeePlayer);
    }

    public override void Sleep()
    {
        base.Sleep();
        ctrl.RegisterAwakenState(null);
        if (anim) anim.ResetTrigger("Awaken");
    }
}
