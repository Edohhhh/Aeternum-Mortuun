using System.Collections;
using UnityEngine;

public class TopoIdleState : State<EnemyInputs>
{
    private readonly TopoController ctrl;
    private readonly float idleSeconds;
    private bool running;

    public TopoIdleState(TopoController c) { ctrl = c; }

    public override void Awake()
    {
        base.Awake();
        running = true;
        if (ctrl.Anim) ctrl.Anim.CrossFade("Idle", 0.05f);
        //ctrl.StartCoroutine(IdleThenAttack());
    }

    private IEnumerator IdleThenAttack()
    {
        yield return new WaitForSeconds(idleSeconds);
        ctrl.Transition(EnemyInputs.SpecialAttack); // AttackState
    }

    public override void Execute()
    {

    }

    public override void Sleep()
    {
        base.Sleep();
        running = false;
    }
}