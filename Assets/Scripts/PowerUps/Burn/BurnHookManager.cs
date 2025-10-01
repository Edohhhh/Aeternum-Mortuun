using UnityEngine;

public class BurnHookManager : MonoBehaviour
{
    private void Update()
    {
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (enemy.GetComponent<EnemyHealth>() != null && enemy.GetComponent<EnemyBurnHook>() == null)
            {
                enemy.AddComponent<EnemyBurnHook>();
            }
        }
    }
}