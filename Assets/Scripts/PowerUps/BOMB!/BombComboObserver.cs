using UnityEngine;

public class BombComboObserver : MonoBehaviour
{
    public PlayerController player;
    public GameObject bombPrefab;
    public float spawnChance = 0.1f;

    private int comboCounter = 0;
    private float comboResetTimer;
    private const float comboResetTime = 1.2f;

    private void Update()
    {
        comboResetTimer -= Time.deltaTime;
        if (comboResetTimer <= 0f)
        {
            comboCounter = 0;
        }

        foreach (var hitbox in GameObject.FindObjectsByType<AttackHitbox>(FindObjectsSortMode.None))
        {
            if (hitbox != null && hitbox.transform.IsChildOf(player.transform))
            {
                if (!hitbox.gameObject.name.Contains("Counted"))
                {
                    hitbox.gameObject.name += "_Counted";
                    RegisterHit();
                }
            }
        }
    }

    private void RegisterHit()
    {
        comboCounter++;
        comboResetTimer = comboResetTime;

        if (comboCounter >= 3)
        {
            comboCounter = 0;

            if (Random.value <= spawnChance)
            {
                SpawnBomb();
            }
        }
    }

    private void SpawnBomb()
    {
        Instantiate(bombPrefab, player.transform.position, Quaternion.identity);
    }
}