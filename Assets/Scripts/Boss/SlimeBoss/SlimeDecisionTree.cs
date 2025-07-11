using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeDecisionTree : MonoBehaviour
{
    private IDesitionNode rootNode;
    private SlimeController slime;

    private void Start()
    {
        slime = GetComponent<SlimeController>();
        CreateTree();
    }

    private void Update()
    {
        if (slime.IsBusyWithSpecial()) return;
        //if (slime.IsInSpecialAttackState()) return;
        //if (slime.IsStunned()) return;
        rootNode.Execute();
    }

    private void CreateTree()
    {

        // Acciones
        ActionNode attack = new ActionNode(Attack);
        ActionNode idle = new ActionNode(Idle);
        ActionNode death = new ActionNode(Die);
        ActionNode special = new ActionNode(SpecialAttack);

        // Sub-nodos organizados como en el ejemplo de tu amigo
        QuestionNode canSee = new QuestionNode(attack, idle, CanSeePlayer);
        QuestionNode canSpecial = new QuestionNode(special, canSee, CanDoSpecialAttack);
        QuestionNode isDead = new QuestionNode(death, canSpecial, IsDead);
        //QuestionNode isDead = new QuestionNode(death, new QuestionNode(attack, idle, CanSeePlayer), IsDead);

        // Pregunta: ¿Puede ver al jugador? (por radio)
        //rootNode = new QuestionNode(attack, idle, CanSeePlayer);
        rootNode = isDead;

    }

    private void Attack()
    {
        //Debug.Log("Slime decisión: ¡Atacar!");
        slime.Transition(EnemyInputs.SeePlayer);
    }

    private void Idle()
    {
        //Debug.Log("Slime decisión: Idle.");
        slime.Transition(EnemyInputs.LostPlayer);


    }

    private void SpecialAttack()
    {
        
        slime.Transition(EnemyInputs.SpecialAttack);
    }

    private bool IsDead()
    {
        return slime.GetCurrentHealth() <= 0;
    }

    private void Die()
    {
        
        slime.Transition(EnemyInputs.Die);
    }

    private bool CanSeePlayer()
    {
        if (slime.GetPlayer() == null) return false;

        float distance = Vector2.Distance(slime.transform.position, slime.GetPlayer().position);
        return distance <= slime.GetDetectionRadius();
    }

    private bool CanDoSpecialAttack()
    {
        return slime.CanUseSpecialAttack();
        //return true;
    }

}