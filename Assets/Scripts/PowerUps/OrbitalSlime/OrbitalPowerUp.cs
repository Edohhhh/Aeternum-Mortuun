using UnityEngine;

[CreateAssetMenu(fileName = "OrbitalPowerUp", menuName = "PowerUps/Orbital Slime")]
public class OrbitalPowerUp : PowerUp
{
    public GameObject orbitalPrefab;
    public float orbitRadius = 1.2f;
    public float rotationSpeed = 90f; // grados por segundo
    public int damagePerSecond = 1;

    private GameObject instance;

    public override void Apply(PlayerController player)
    {
        if (instance != null) return;

        instance = GameObject.Instantiate(orbitalPrefab);
        var orbital = instance.AddComponent<PlayerOrbital>();
        orbital.Initialize(player.transform, orbitRadius, rotationSpeed, damagePerSecond);

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
