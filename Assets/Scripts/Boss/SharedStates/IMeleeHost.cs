using UnityEngine;

public interface IMeleeHost
{
    Transform Transform { get; }
    Animator Animator { get; }
    Rigidbody2D Body { get; }

    // D�nde est� el script de da�o (en el prefab del enemigo)
    EnemyAttack Attack { get; }

    // FSM
    void Transition(EnemyInputs input);

    // Para reenviar Animation Events (igual que con Burrow/Spawn)
    void RegisterMeleeState(MeleeAttackState state);
}
