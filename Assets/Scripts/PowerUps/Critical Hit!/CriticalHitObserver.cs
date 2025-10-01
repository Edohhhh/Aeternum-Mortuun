using UnityEngine;

public class CriticalHitObserver : MonoBehaviour
{
    public float critChance = 0.05f;

    public bool ShouldApplyCrit()
    {
        return Random.value < critChance;
    }

    private void Update()
    {
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (enemy.GetComponent<EnemyCritHook>() == null)
            {
                var health = enemy.GetComponent<EnemyHealth>();
                if (health != null)
                {
                    var hook = enemy.AddComponent<EnemyCritHook>();
                    hook.Init(this, health);
                }
            }
        }
    }
}
