using UnityEngine;

[CreateAssetMenu(fileName = "OrbitalPowerUp", menuName = "PowerUps/Orbital Slime")]
public class OrbitalPowerUp : PowerUp
{
    public GameObject orbitalPrefab;
    public float orbitRadius = 1.2f;
    public float rotationSpeed = 90f; // grados por segundo
    public int damagePerSecond = 1;

    public override void Apply(PlayerController player)
    {
        var manager = Object.FindFirstObjectByType<OrbitalManager>();
        if (manager == null)
        {
            GameObject go = new GameObject("OrbitalManager");
            manager = go.AddComponent<OrbitalManager>();
            Object.DontDestroyOnLoad(go);
        }

        manager.SpawnOrbital(this, player);
    }

    public override void Remove(PlayerController player)
    {
        var manager = Object.FindFirstObjectByType<OrbitalManager>();
        if (manager != null)
        {
            manager.DestroyOrbital();
        }
    }
}