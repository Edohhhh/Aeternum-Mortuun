using UnityEngine;

public interface IEnemyDataProvider
{
    Transform GetPlayer();
    float GetDetectionRadius();
    float GetAttackDistance();
    float GetDamage();
    float GetMaxSpeed();
    float GetAcceleration();

    void Transition(EnemyInputs input);
}
