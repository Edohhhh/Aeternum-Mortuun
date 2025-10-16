using UnityEngine;

[CreateAssetMenu(fileName = "OrbitalPowerUp", menuName = "PowerUps/Orbital Slime")]
public class OrbitalPowerUp : PowerUp
{
    public GameObject orbitalPrefab;
    public float orbitRadius = 1.2f;
    public float rotationSpeed = 90f;
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

        // En lugar de reemplazar, acumulamos
        manager.AddStack(this, player);
    }

    public override void Remove(PlayerController player)
    {
        var manager = Object.FindFirstObjectByType<OrbitalManager>();
        if (manager != null)
        {
            manager.ClearOrbitals();
        }
    }
}