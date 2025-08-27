using UnityEngine;

[CreateAssetMenu(fileName = "CursedCompanionPowerUp", menuName = "PowerUps/Cursed Companion")]
public class CursedCompanionPowerUp : PowerUp
{
    [Header("Configuración del Compañero")]
    public GameObject companionPrefab;
    public GameObject bulletPrefab;

    [Header("Configuración de Disparo")]
    public float fireInterval = 5f; // Tiempo entre ráfagas
    public float burstInterval = 0.1f; // Tiempo entre balas en la ráfaga
    public int bulletsPerBurst = 3; // Número de balas por ráfaga
    public int damagePerShot = 1;

    [Header("Configuración de Movimiento")]
    public float speedMultiplier = 0.75f;

    private GameObject instance;

    public override void Apply(PlayerController player)
    {
        if (instance != null) return;

        instance = GameObject.Instantiate(companionPrefab);
        var companion = instance.AddComponent<CursedCompanion>();
        companion.bulletPrefab = bulletPrefab;
        companion.Initialize(player.transform, player.moveSpeed * speedMultiplier, fireInterval, damagePerShot);
        companion.burstInterval = burstInterval;
        companion.bulletsPerBurst = bulletsPerBurst;

        Object.DontDestroyOnLoad(instance);
        Debug.Log($"[CURSED COMPANION] ¡Compañero maldito activado! Ráfagas de {bulletsPerBurst} balas cada {fireInterval} segundos");
    }

    public override void Remove(PlayerController player)
    {
        if (instance != null)
        {
            GameObject.Destroy(instance);
            instance = null;
            Debug.Log("[CURSED COMPANION] Compañero maldito desactivado");
        }
    }
}