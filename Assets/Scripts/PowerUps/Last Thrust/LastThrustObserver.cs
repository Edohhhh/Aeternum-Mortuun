using UnityEngine;

public class LastThrustObserver : MonoBehaviour
{
    public PlayerController player;
    public GameObject shockwavePrefab;

    private int comboCounter = 0;
    private float comboTimer = 0f;
    private const float comboResetTime = 1.2f;

    void Update()
    {
        comboTimer -= Time.deltaTime;
        if (comboTimer <= 0f) comboCounter = 0;

        foreach (var hitbox in GameObject.FindObjectsByType<AttackHitbox>(FindObjectsSortMode.None))
        {
            if (hitbox != null && hitbox.transform.IsChildOf(player.transform) && !hitbox.name.Contains("CountedThrust"))
            {
                hitbox.name += "_CountedThrust";
                RegisterHit();
            }
        }
    }

    void RegisterHit()
    {
        comboCounter++;
        comboTimer = comboResetTime;

        if (comboCounter >= 3)
        {
            comboCounter = 0;
            SpawnShockwave();
        }
    }

    void SpawnShockwave()
    {
        Instantiate(shockwavePrefab, player.transform.position, Quaternion.identity);
    }
}
