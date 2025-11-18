using UnityEngine;

public class ArqueroDecisionTree : MonoBehaviour
{
    private IDesitionNode rootNode;
    private ArqueroController boss; // Asegúrate de que apunte a ArqueroController

    private void Start()
    {
        boss = GetComponent<ArqueroController>();
        CreateTree();
    }

    private void Update()
    {
        // No tomar decisiones si está spawneando o ya disparando
        if (boss.IsSpawning() || boss.IsShooting())
            return;

        rootNode.Execute();
    }

    private void CreateTree()
    {
        // 1. Acciones
        var deathAction = new ActionNode(() => boss.Transition(EnemyInputs.Die));

        // Usamos MeleeAttack como input para disparar (para no modificar el enum)
        var shootAction = new ActionNode(() => boss.Transition(EnemyInputs.MeleeAttack));

        var idleAction = new ActionNode(() => boss.Transition(EnemyInputs.LostPlayer));

        // 2. Decisiones

        // Pregunta 2: ¿Puedo disparar Y veo al jugador?
        var canShoot = new QuestionNode(
            shootAction, // Si (True)
            idleAction,  // No (False)
            () => boss.CanShoot() && boss.IsPlayerInDetectionRange() // Condición
        );

        // Pregunta 1 (Raíz): ¿Estoy muerto?
        var isDead = new QuestionNode(
            deathAction, // Si (True)
            canShoot,    // No (False) -> Pasa a la siguiente pregunta
            IsDead
        );

        rootNode = isDead;
    }

    private bool IsDead()
    {
        return boss.GetCurrentHealth() <= 0f;
    }
}