using UnityEngine;

public class DiabloAttackState : State<EnemyInputs>
{
    private readonly DiabloController ctrl;
    public DiabloAttackState(DiabloController c) { ctrl = c; }

    public override void Awake()
    {
        base.Awake();
        ctrl.RegisterAttackState(this);

        int id = Mathf.Clamp(ctrl.Roll, 1, ctrl.AnimCount);
        if (ctrl.Anim)
        {
            string trig = $"Attack{id}";
            ctrl.Anim.ResetTrigger(trig);
            ctrl.Anim.SetTrigger(trig);
        }
    }

    // Llamado por DiabloController.OnAttackEnd() (Animation Event)
    public void OnAttackFinished()
    {
        ctrl.Transition(EnemyInputs.SeePlayer); //  Idle
    }
}

