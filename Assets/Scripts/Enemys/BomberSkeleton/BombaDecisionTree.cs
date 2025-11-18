using UnityEngine;

public class BombaDecisionTree : MonoBehaviour
{
    private IDesitionNode rootNode;
    private BombaController boss;

    private void Start()
    {
        boss = GetComponent<BombaController>();
        CreateTree();
    }

    private void Update()
    {
        if (boss.IsSpawning() || boss.IsExploding())
            return;

        rootNode.Execute();
    }

    // Asegúrate de que tu CreateTree tenga esta lógica
    private void CreateTree()
    {
        // 1. Acciones
        var deathAction = new ActionNode(() => boss.Transition(EnemyInputs.Die));
        var explodeAction = new ActionNode(() => boss.Transition(EnemyInputs.Explode));
        var followAction = new ActionNode(() => boss.Transition(EnemyInputs.SeePlayer));
        var idleAction = new ActionNode(() => boss.Transition(EnemyInputs.LostPlayer));

        // 2. Decisiones

        // Pregunta 4: ¿Veo al jugador (rango exterior)?
        var canSee = new QuestionNode(followAction, idleAction, boss.IsPlayerInDetectionRange);

        // Pregunta 3: ¿Está el jugador DEMASIADO cerca (rango interior)?
        // Esta función 'IsPlayerInStandoffRange' ahora lee la bandera del trigger
        var isTooClose = new QuestionNode(idleAction, canSee, boss.IsPlayerInStandoffRange);

        // Pregunta 2: ¿Se acabó el tiempo?
        var isTimerDone = new QuestionNode(explodeAction, isTooClose, boss.IsExplosionTimerDone);

        // Pregunta 1 (Raíz): ¿Estoy muerto?
        var isDead = new QuestionNode(deathAction, isTimerDone, IsDead);

        rootNode = isDead;
    }

    private bool IsDead()
    {
        return boss.GetCurrentHealth() <= 0f;
    }
}