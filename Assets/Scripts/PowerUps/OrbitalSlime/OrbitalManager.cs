using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class OrbitalManager : MonoBehaviour
{
    private List<GameObject> orbitals = new List<GameObject>();
    private OrbitalPowerUp config;
    private PlayerController player;
    private int stackCount = 0;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void AddStack(OrbitalPowerUp config, PlayerController player)
    {
        this.config = config;
        this.player = player;

        stackCount++;
        RebuildOrbitals();
    }

    private void RebuildOrbitals()
    {
        // Limpiar los anteriores
        foreach (var orb in orbitals)
            if (orb != null) Destroy(orb);

        orbitals.Clear();

        // Crear todos los orbitales según el stack
        for (int i = 0; i < stackCount; i++)
        {
            var go = Instantiate(config.orbitalPrefab);
            var orbital = go.AddComponent<PlayerOrbital>();

            float angleOffset = (360f / stackCount) * i;
            float adjustedSpeed = config.rotationSpeed * Mathf.Sign((i % 2 == 0) ? 1 : -1);

            orbital.Initialize(player.transform, config.orbitRadius, adjustedSpeed, config.damagePerSecond);
            orbital.SetInitialAngle(angleOffset);

            orbitals.Add(go);
        }
    }

    public void ClearOrbitals()
    {
        foreach (var orb in orbitals)
            if (orb != null) Destroy(orb);

        orbitals.Clear();
        stackCount = 0;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Al cambiar de escena, volver a asignar al jugador
        StartCoroutine(ReassignPlayer());
    }

    private System.Collections.IEnumerator ReassignPlayer()
    {
        PlayerController found = null;
        while (found == null)
        {
            found = Object.FindFirstObjectByType<PlayerController>();
            yield return null;
        }

        player = found;
        RebuildOrbitals();
    }
}