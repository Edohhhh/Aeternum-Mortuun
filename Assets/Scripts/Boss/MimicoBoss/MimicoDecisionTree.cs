using UnityEngine;

public class MimicoDecisionTree : MonoBehaviour
{
    private IDesitionNode root;
    private MimicoController boss;

    private void Start()
    {
        boss = GetComponent<MimicoController>();
        Build();
    }

    private void Update()
    {
        if (boss.IsDormant() || boss.IsAwakening() || boss.IsMeleeing() || boss.IsSpecialing())
            return; // no interrumpir la anim de Melee
        root.Execute();
    }

    private void Build()
    {
        var die = new ActionNode(() => boss.Transition(EnemyInputs.Die));
        var melee = new ActionNode(() => { boss.MarkMeleeUsed(); boss.Transition(EnemyInputs.MeleeAttack); });
        var follow = new ActionNode(() => boss.Transition(EnemyInputs.SeePlayer));
        var idle = new ActionNode(() => boss.Transition(EnemyInputs.LostPlayer));
        var special = new ActionNode(() => boss.Transition(EnemyInputs.SpecialAttack));

        // Igual que Golem/Skeleton: primero ver al jugador
        var canSee = new QuestionNode(follow, idle, CanSeePlayer);

        // Luego Melee (sin exigir IsFollowing)
        var canMelee = new QuestionNode(
            melee,
            canSee,
            () => boss.IsPlayerInMeleeRange() && boss.CanMeleeAttack()
        );

        var canSpecial = new QuestionNode(special, canMelee, () => boss.CanUseSpecial() && boss.IsFollowing());

        // Raíz: muerte primero
        root = new QuestionNode(die,canSpecial, IsDead);
    }

    private bool IsDead() => boss.GetCurrentHealth() <= 0f;

    private bool CanSeePlayer()
    {
        var p = boss.GetPlayer();
        return p != null &&
               Vector2.Distance(boss.transform.position, p.position) <= boss.GetDetectionRadius();
    }
}