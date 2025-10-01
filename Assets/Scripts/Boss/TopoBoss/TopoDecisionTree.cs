using UnityEngine;

public class TopoDecisionTree : MonoBehaviour
{
    private IDesitionNode root;
    private TopoController topo;


    private void Awake() { topo = GetComponent<TopoController>(); }
    private void Start()
    {
        topo = GetComponent<TopoController>();
        Build();
    }

    private void Update()
    {
        if (topo && topo.GetCurrentHealth() <= 0f)
            topo.Transition(EnemyInputs.Die);
        //root.Execute();
    }

    private void Build()
    {
        var die = new ActionNode(() => topo.Transition(EnemyInputs.Die));
        var see = new ActionNode(() => topo.Transition(EnemyInputs.SeePlayer));
        var idle = new ActionNode(() => topo.Transition(EnemyInputs.LostPlayer));

        var canSee = new QuestionNode(see, idle, CanSeePlayer);
        var isDead = new QuestionNode(die, canSee, IsDead);

        root = isDead;
    }

    private bool IsDead() => topo.GetCurrentHealth() <= 0f;

    private bool CanSeePlayer()
    {
        var p = topo.GetPlayer();
        return p && Vector2.Distance(topo.transform.position, p.position) <= topo.GetDetectionRadius();
    }
}

