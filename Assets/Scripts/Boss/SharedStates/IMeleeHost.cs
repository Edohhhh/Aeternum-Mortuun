using UnityEngine;

public interface IMeleeHost
{
    Transform Transform { get; }
    Animator Animator { get; }
    Rigidbody2D Body { get; }

    // Dónde está el script de daño (en el prefab del enemigo)
    EnemyAttack Attack { get; }

    // FSM
    void Transition(EnemyInputs input);

    // Para reenviar Animation Events (igual que con Burrow/Spawn)
    void RegisterMeleeState(MeleeAttackState state);
}
