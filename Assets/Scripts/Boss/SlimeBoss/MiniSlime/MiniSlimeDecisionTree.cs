using UnityEngine;

public class MiniSlimeDecisionTree : MonoBehaviour
{
    private IDesitionNode rootNode;
    private MiniSlimeController slime;

    private void Start()
    {
        slime = GetComponent<MiniSlimeController>();
        CreateTree();
    }

    private void Update()
    {
        //if (slime.IsStunned()) return;
        rootNode.Execute();
    }

    private void CreateTree()
    {
        ActionNode attack = new ActionNode(Attack);
        ActionNode idle = new ActionNode(Idle);
        ActionNode die = new ActionNode(Die);

        QuestionNode isDead = new QuestionNode(die, new QuestionNode(attack, idle, CanSeePlayer), IsDead);
        rootNode = isDead;
    }

    private void Attack() => slime.Transition(EnemyInputs.SeePlayer);
    private void Idle() => slime.Transition(EnemyInputs.LostPlayer);
    private void Die() => slime.Transition(EnemyInputs.Die);

    private bool CanSeePlayer()
    {
        if (slime.GetPlayer() == null) return false;
        float distance = Vector2.Distance(slime.transform.position, slime.GetPlayer().position);
        return distance <= slime.GetDetectionRadius();
    }

    private bool IsDead()
    {
        return slime.GetCurrentHealth() <= 0;
    }
}