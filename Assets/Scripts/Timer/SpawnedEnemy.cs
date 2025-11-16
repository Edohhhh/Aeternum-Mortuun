using UnityEngine;

public class SpawnedEnemy : MonoBehaviour
{
    [HideInInspector]
    public WaveManager manager;

    private void OnDestroy()
    {
        if (manager != null)
        {
            manager.NotifyEnemyDestroyed();
        }
    }
}
