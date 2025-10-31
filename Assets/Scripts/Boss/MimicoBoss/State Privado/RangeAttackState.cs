using UnityEngine;

public class RangeAttackState : State<EnemyInputs>
{
    private readonly MimicoController ctrl;

    private Rigidbody2D rb;
    private Animator anim;
    private RigidbodyConstraints2D saved;
    private bool finished;

    public RangeAttackState(MimicoController c) { ctrl = c; }

    public override void Awake()
    {
        base.Awake();
        finished = false;

        rb = ctrl.Body;
        anim = ctrl.Animator;

        // Congelar posición mientras dispara (opcional)
        if (rb != null)
        {
            saved = rb.constraints;
            rb.linearVelocity = Vector2.zero;
            rb.constraints = saved | RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
        }

        // Registrar este estado para que reciba los Animation Events
        ctrl.RegisterRangeState(this);

        // Comienza el cooldown inmediatamente al entrar al estado
        ctrl.MarkRangeUsed();

        // Dispara la animación de disparo
        if (anim != null)
        {
            anim.ResetTrigger("Melee");
            anim.ResetTrigger("Awaken");
            anim.ResetTrigger("Range");   // aseguramos un set limpio
            anim.SetTrigger("Range");     // tu clip de disparo
        }
    }

    // Llamado por el Animation Event del clip: AE_OnAttackFire
    public void OnRangeFire()
    {
        ctrl.SpawnRangeProjectile();
    }

    // Llamado por el Animation Event del clip: AE_OnAttackEnd
    public void OnRangeFinished()
    {
        finished = true;
    }

    public override void Execute()
    {
        if (!finished) return;

        // Liberar congelamiento antes de salir
        if (rb != null)
            rb.constraints = saved;

        ctrl.Transition(EnemyInputs.SeePlayer);
    }

    public override void Sleep()
    {
        base.Sleep();
        ctrl.RegisterRangeState(null);
        if (anim != null) anim.ResetTrigger("Range");
    }
}

