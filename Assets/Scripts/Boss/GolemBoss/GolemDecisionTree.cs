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
        if (golem.IsMeleeing() || golem.IsLasering() || golem.IsHeavying())
            return;

        root.Execute();
    }

    private void Build()
    {
        var die = new ActionNode(() => golem.Transition(EnemyInputs.Die));
        var melee = new ActionNode(() => { golem.MarkMeleeUsed(); golem.Transition(EnemyInputs.MeleeAttack); });
        var heavy = new ActionNode(() => { golem.Transition(EnemyInputs.HeavyAttack); });
        var laser = new ActionNode(() => golem.Transition(EnemyInputs.SpecialAttack));
        var follow = new ActionNode(() => golem.Transition(EnemyInputs.SeePlayer));
        var idle = new ActionNode(() => golem.Transition(EnemyInputs.LostPlayer));

        var canSee = new QuestionNode(follow, idle, CanSeePlayer);
        var canMelee = new QuestionNode(melee, canSee, () => golem.IsPlayerInMeleeRange() && golem.CanMeleeAttack());

        // PRIMERO láser, DESPUÉS heavy
        var canLaser = new QuestionNode(
            laser,
            canMelee,
            () => golem.CanUseLaser() && golem.IsPlayerOutsideMelee() && golem.IsFollowing()
        );

        var canHeavy = new QuestionNode(
            heavy,
            canLaser,
            () => golem.IsPlayerInHeavyRange() && golem.CanUseHeavy() && golem.IsFollowing()
        );

        var isDead = new QuestionNode(die, canHeavy, IsDead);

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
