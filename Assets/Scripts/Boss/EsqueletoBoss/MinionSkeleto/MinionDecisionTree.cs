using UnityEngine;

public class MinionDecisionTree : MonoBehaviour
{
    private IDesitionNode rootNode;
    private MinionController minion;

    private void Start()
    {
        minion = GetComponent<MinionController>();
        CreateTree();
    }

    private void Update()
    {
        if (minion.IsSpawning() || minion.IsMeleeing())
            return;

        rootNode.Execute();
    }

    private void CreateTree()
    {
        var death = new ActionNode(() => minion.Transition(EnemyInputs.Die));
        var melee = new ActionNode(() => { minion.MarkMeleeUsed(); minion.Transition(EnemyInputs.MeleeAttack); });
        var follow = new ActionNode(() => minion.Transition(EnemyInputs.SeePlayer));
        var idle = new ActionNode(() => minion.Transition(EnemyInputs.LostPlayer));

        var canSee = new QuestionNode(follow, idle, CanSeePlayer);
        var canMelee = new QuestionNode(melee, canSee, () => minion.IsPlayerInMeleeRange() && minion.CanMeleeAttack());

        // prioridad: morir > melee > follow/idle
        rootNode = new QuestionNode(death, canMelee, IsDead);
    }

    private bool IsDead() => minion.GetCurrentHealth() <= 0f;

    private bool CanSeePlayer()
    {
        var p = minion.GetPlayer();
        return p != null
            && Vector2.Distance(minion.transform.position, p.position)
               <= minion.GetDetectionRadius();
    }
}