using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonDecisionTree : MonoBehaviour
{
    private IDesitionNode rootNode;
    private SkeletonController boss;

    private void Start()
    {
        boss = GetComponent<SkeletonController>();
        CreateTree();
    }


    private void Update()
    {
        if (boss.IsSpawning() || boss.IsSpawningMinions())
            return;
        rootNode.Execute();
    }

    private void CreateTree()
    {
        var deathAction = new ActionNode(() => boss.Transition(EnemyInputs.Die));
        var ugAction = new ActionNode(() => {
            boss.MarkUnderGroundUsed();
            boss.Transition(EnemyInputs.UnderGroundAttack);
        });
        var spawnMAction = new ActionNode(() => {
            boss.MarkSpawnMinionsUsed();
            boss.Transition(EnemyInputs.SpawnMinions);
        });
        var followAction = new ActionNode(() => boss.Transition(EnemyInputs.SeePlayer));
        var idleAction = new ActionNode(() => boss.Transition(EnemyInputs.LostPlayer));

        // Preguntas (QuestionNodes) en orden de prioridad
        var canSee = new QuestionNode(followAction, idleAction, CanSeePlayer);
        var canSpawn = new QuestionNode(spawnMAction, canSee, boss.CanUseSpawnMinions);
        var canUndrGr = new QuestionNode(ugAction, canSpawn, boss.CanUseUnderGroundAttack);
        var isDead = new QuestionNode(deathAction, canUndrGr, IsDead);

        rootNode = isDead;
    }

    private bool IsDead()
    {
        return boss.GetCurrentHealth() <= 0f;
    }



    private bool CanSeePlayer()
    {
        var p = boss.GetPlayer();
        return p != null &&
               Vector2.Distance(boss.transform.position, p.position) <= boss.GetDetectionRadius();
    }
}


