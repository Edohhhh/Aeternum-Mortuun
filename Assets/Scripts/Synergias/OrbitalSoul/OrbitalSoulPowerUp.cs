using UnityEngine;

[CreateAssetMenu(fileName = "OrbitalSoulPowerUp", menuName = "PowerUps/Orbital Soul")]
public class OrbitalSoulPowerUp : PowerUp
{
    [Header("Configuración de órbita")]
    public GameObject orbitalSoulPrefab;
    public float orbitRadius = 1.2f;
    public float rotationSpeed = 90f;

    [Header("Configuración de daño por contacto")]
    public int contactDamage = 1;
    public float contactDamageInterval = 1f;

    [Header("Configuración de proyectiles")]
    public GameObject bulletPrefab;
    public float shootInterval = 2f;
    public float bulletSpeed = 5f;
    public int bulletDamage = 1;

    private GameObject instance;

    public override void Apply(PlayerController player)
    {
        if (instance != null) return;

        instance = GameObject.Instantiate(orbitalSoulPrefab);
        var orbitalSoul = instance.AddComponent<OrbitalSoul>();
        orbitalSoul.Initialize(
            player.transform,
            orbitRadius,
            rotationSpeed,
            contactDamage,
            contactDamageInterval,
            bulletPrefab,
            shootInterval,
            bulletSpeed,
            bulletDamage
        );

        Object.DontDestroyOnLoad(instance);
    }

    public override void Remove(PlayerController player)
    {
        if (instance != null)
        {
            Object.Destroy(instance);
            instance = null;
        }
    }
}