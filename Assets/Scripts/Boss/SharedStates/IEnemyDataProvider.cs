using UnityEngine;

public interface IEnemyDataProvider
{
    Transform GetPlayer();
    float GetMaxSpeed();
    float GetAcceleration();
}
