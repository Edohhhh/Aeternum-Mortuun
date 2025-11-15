using UnityEngine;

// Asumo que tienes clases base:
// public interface IDesitionNode { void Execute(); }
// public class ActionNode : IDesitionNode { ... }
// public class QuestionNode : IDesitionNode { ... }

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
        // No tomar decisiones si está spawneando o ya está en proceso de explotar/morir
        if (boss.IsSpawning() || boss.IsExploding())
            return;

        rootNode.Execute();
    }

    private void CreateTree()
    {
        // 1. Acciones (Nodos Hoja)
        var deathAction = new ActionNode(() => boss.Transition(EnemyInputs.Die));
        var explodeAction = new ActionNode(() => boss.Transition(EnemyInputs.Explode));
        var followAction = new ActionNode(() => boss.Transition(EnemyInputs.SeePlayer));
        var idleAction = new ActionNode(() => boss.Transition(EnemyInputs.LostPlayer));

        // 2. Decisiones (Nodos Rama)

        // Pregunta 3: ¿Veo al jugador?
        // Si sí -> Sigue (followAction)
        // Si no -> Quieto (idleAction)
        var canSee = new QuestionNode(followAction, idleAction, boss.IsPlayerInDetectionRange);

        // Pregunta 2: ¿Se acabó el tiempo?
        // Si sí -> Explota (explodeAction)
        // Si no -> Pregunta 3 (canSee)
        var isTimerDone = new QuestionNode(explodeAction, canSee, boss.IsExplosionTimerDone);

        // Pregunta 1 (Raíz): ¿Estoy muerto?
        // Si sí -> Muere (deathAction)
        // Si no -> Pregunta 2 (isTimerDone)
        var isDead = new QuestionNode(deathAction, isTimerDone, IsDead);

        rootNode = isDead;
    }

    private bool IsDead()
    {
        return boss.GetCurrentHealth() <= 0f;
    }
}