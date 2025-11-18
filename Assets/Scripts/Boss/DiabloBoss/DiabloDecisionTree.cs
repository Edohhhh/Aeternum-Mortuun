using UnityEngine;

public class DiabloDecisionTree : MonoBehaviour
{
    private IDesitionNode root;
    private DiabloController boss;

    private void Start()
    {
        boss = GetComponent<DiabloController>();
        Build();
    }

    private void Update()
    {
        // Igual que en Mimico/Golem: si está ejecutando anim/ataque, no lo interrumpimos
        if (boss.IsAnimating() || boss.IsAttacking())
            return;

        root.Execute();
    }

    private void Build()
    {
        var die = new ActionNode(() => boss.Transition(EnemyInputs.Die));
        var start = new ActionNode(() => { boss.MarkCycleStarted(); boss.Transition(EnemyInputs.SpecialAttack); });
        var noop = new ActionNode(() => { /* no hace nada, sigue en Idle cargando */ });

        // si puede morir  Die
        // si cargó la pausa  arrancar ciclo (Idle  Random)
        // si no  no-op
        var canStart = new QuestionNode(start, noop, () => boss.IsIdling() && boss.CanStartCycle());
        root = new QuestionNode(die, canStart, () => boss.HP <= 0f);
    }
}