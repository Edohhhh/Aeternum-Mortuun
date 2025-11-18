using UnityEngine;

public class DiabloAttackRouterState : State<EnemyInputs>
{
    private readonly DiabloController ctrl;
    private IDiabloAttack attack;

    public DiabloAttackRouterState(DiabloController c) { ctrl = c; }

    public override void Awake()
    {
        base.Awake();
        Debug.Log("[FSM] ENTER: DiabloAttackRouterState");
        if (ctrl.Anim) ctrl.Anim.SetBool("isAttacking", true);
        attack = DiabloAttackFactory.Create(ctrl.Roll);
        attack?.Start(ctrl);
    }

    public override void Execute()
    {
        attack?.Tick(ctrl);
        if (attack == null || attack.IsFinished)
            ctrl.Transition(EnemyInputs.SeePlayer); 
    }

    public override void Sleep()
    {
        attack?.Stop(ctrl);
        if (ctrl.Anim) ctrl.Anim.SetBool("isAttacking", false);
    }
}

