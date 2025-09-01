using UnityEngine;

public class GolemDecisionTree : MonoBehaviour
{
    private IDesitionNode root;
    private GolemController golem;

    private void Start()
    {
        golem = GetComponent<GolemController>();
        Build();
    }

    private void Update()
    {
        if (golem.IsMeleeing() || golem.IsLasering())
            return;

        root.Execute();
    }

    private void Build()
    {
        var die = new ActionNode(() => golem.Transition(EnemyInputs.Die));
        var melee = new ActionNode(() => { golem.MarkMeleeUsed(); golem.Transition(EnemyInputs.MeleeAttack); });
        var special = new ActionNode(() => golem.Transition(EnemyInputs.SpecialAttack));
        var follow = new ActionNode(() => golem.Transition(EnemyInputs.SeePlayer));
        var idle = new ActionNode(() => golem.Transition(EnemyInputs.LostPlayer));

        var canSee = new QuestionNode(follow, idle, CanSeePlayer);
        var canSpecial = new QuestionNode(
        special,
        canSee,
        () => golem.CanUseLaser() && golem.IsPlayerOutsideMelee() && golem.IsFollowing()
    );
        var canMelee = new QuestionNode(
        melee,
        canSpecial,
        () => golem.IsPlayerInMeleeRange() && golem.CanMeleeAttack()
    );
        //var canMelee = new QuestionNode(melee, canSee, () => golem.IsPlayerInMeleeRange() && golem.CanMeleeAttack());
        var isDead = new QuestionNode(die, canMelee, IsDead);

        root = isDead;
    }

    private bool IsDead()
    {
        return golem.GetCurrentHealth() <= 0f;
    }

    private bool CanSeePlayer()
    {
        var p = golem.GetPlayer();
        return p != null &&
               Vector2.Distance(golem.transform.position, p.position) <= golem.GetDetectionRadius();
    }
}
