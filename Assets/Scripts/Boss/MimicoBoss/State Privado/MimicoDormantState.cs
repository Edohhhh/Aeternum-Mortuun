using UnityEngine;

public class MimicoDormantState : State<EnemyInputs>
{
    private readonly MimicoController ctrl;

    private Rigidbody2D rb;
    private Animator anim;
    private RigidbodyConstraints2D saved;
    private bool freezeSet;

    public MimicoDormantState(MimicoController c) { ctrl = c; }

    public override void Awake()
    {
        base.Awake();
        rb = ctrl.GetComponent<Rigidbody2D>();
        anim = ctrl.Animator;

        // Quedarse quieto: no moverse ni rotar
        if (rb)
        {
            saved = rb.constraints;
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
            freezeSet = true;
        }

        ctrl.SetBossCollidersEnabled(false);
        ctrl.SetDummyColliderActive(true);
        // Icono “F” visible SOLO en Dormant
        ctrl.EnsureDormantIcon();
        ctrl.SetDormantIconVisible(true);


        // Anim de “quieto/mímico”
        if (anim)
        {
            anim.SetBool("isWalking", false);
            anim.ResetTrigger("Melee");
            anim.ResetTrigger("Die");
            anim.ResetTrigger("Awaken");
            // Usa un clip looping: "Dormant" (o tu "Idle" estático del mímico)
            anim.Play("Dormant", 0, 0f);
        }
    }

    public override void Execute()
    {
        // No reaccionar a ver al jugador. Solo permitir interacción F muy cerca.
        var p = ctrl.GetPlayer();
        if (!p) return;

        float dist = Vector2.Distance(ctrl.transform.position, p.position);
        if (dist <= ctrl.InteractRadius && Input.GetKeyDown(ctrl.InteractKey))
        {
            // Pasar a “despertar”
            ctrl.Transition(EnemyInputs.SpecialAttack);
        }
    }

    public override void Sleep()
    {
        base.Sleep();
        // Restaurar físicas al salir
        ctrl.SetDormantIconVisible(false);

        if (freezeSet && rb)
        {
            rb.constraints = saved;
            freezeSet = false;
        }
    }
}
