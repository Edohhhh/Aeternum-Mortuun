using UnityEngine;

public class MeleeAttackState : State<EnemyInputs>
{
    private readonly IMeleeHost host;

    private RigidbodyConstraints2D saved;
    private bool finished;

    public MeleeAttackState(IMeleeHost host) { this.host = host; }

    public override void Awake()
    {
        base.Awake();
        finished = false;

        // Congelar movimiento durante el golpe
        if (host.Body != null)
        {
            saved = host.Body.constraints;
            host.Body.linearVelocity = Vector2.zero;
            host.Body.constraints = saved | RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
        }

        // Disparar animación genérica (usa el mismo trigger en todos los enemigos)
        host.Animator.ResetTrigger("Melee");
        host.Animator.SetTrigger("Melee");

        // Registrar para recibir eventos reenviados
        host.RegisterMeleeState(this);
    }

    // Llamado por Animation Event "OnMeleeHit" (reenviado por el host)
    public void OnMeleeHit()
    {
        Debug.Log("MELEE: OnMeleeHit()");
        host.Attack?.DoDamageManual(); // usa el EnemyAttack del prefab
    }

    // Llamado por Animation Event "OnMeleeFinished"
    public void OnMeleeFinished()
    {
        finished = true;
    }

    public override void Execute()
    {
        if (!finished) return;
        host.Transition(EnemyInputs.SeePlayer);
    }

    public override void Sleep()
    {
        if (host.Body != null)
            host.Body.constraints = saved | RigidbodyConstraints2D.FreezeRotation;
        host.RegisterMeleeState(null);
        base.Sleep();
    }
}
