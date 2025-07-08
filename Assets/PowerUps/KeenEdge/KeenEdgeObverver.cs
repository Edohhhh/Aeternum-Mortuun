using UnityEngine;

public class KeenEdgeObserver : MonoBehaviour
{
    public float bleedChance = 0.05f;
    public int bleedStartDamage = 3;

    private void Update()
    {
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (enemy == null) continue;
            if (!enemy.TryGetComponent(out EnemyHealth health)) continue;

            if (enemy.GetComponent<KeenEdgeHook>() == null)
            {
                var hook = enemy.AddComponent<KeenEdgeHook>();
                hook.Initialize(bleedChance, bleedStartDamage);
            }
        }
    }
}
