using UnityEngine;

public class BloodRitualObserver : MonoBehaviour
{
    public float healChance = 0.1f;
    public float healAmount = 0.5f;
    public PlayerController player;

    private void Update()
    {
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (enemy == null) continue;

            if (enemy.TryGetComponent(out EnemyHealth health))
            {
                if (enemy.GetComponent<BloodRitualHook>() == null)
                {
                    var hook = enemy.AddComponent<BloodRitualHook>();
                    hook.Initialize(health, player, healChance, healAmount);
                }
            }
        }
    }
}
