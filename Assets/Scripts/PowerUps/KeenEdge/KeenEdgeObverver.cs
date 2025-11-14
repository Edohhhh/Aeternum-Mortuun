using UnityEngine;

public class KeenEdgeObserver : MonoBehaviour
{
    [Header("Bleed Settings")]
    public float bleedChance = 0.05f;
    public int bleedStartDamage = 3;
    public float bleedCooldown = 2f;

    [Header("VFX")]
    public GameObject bleedVfxPrefab;

    private void Update()
    {
        // Busca enemigos por tag "Enemy"
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            if (!enemy.TryGetComponent(out EnemyHealth health)) continue;

            if (!enemy.TryGetComponent(out KeenEdgeHook hook))
            {
                hook = enemy.AddComponent<KeenEdgeHook>();
                hook.Initialize(bleedChance, bleedStartDamage, bleedVfxPrefab, bleedCooldown);
            }
        }
    }
}
