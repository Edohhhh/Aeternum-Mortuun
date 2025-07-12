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
        // <-- aqu� a�adimos la guardia
        if (minion.IsSpawning())
            return;

        // S�lo cuando ya sali� del spawn ejecutamos el �rbol
        rootNode.Execute();
    }

    private void CreateTree()
    {
        var death = new ActionNode(() => minion.Transition(EnemyInputs.Die));
        var follow = new ActionNode(() => minion.Transition(EnemyInputs.SeePlayer));
        var idle = new ActionNode(() => minion.Transition(EnemyInputs.LostPlayer));

        var canSee = new QuestionNode(follow, idle, CanSeePlayer);
        rootNode = new QuestionNode(death, canSee, IsDead);
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