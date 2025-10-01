using UnityEngine;
using UnityEngine.SceneManagement;

public class OrbitalManager : MonoBehaviour
{
    private GameObject orbitalInstance;
    private OrbitalPowerUp config;
    private PlayerController player;

    public void SpawnOrbital(OrbitalPowerUp config, PlayerController player)
    {
        this.config = config;
        this.player = player;

        if (orbitalInstance != null) return;

        orbitalInstance = Instantiate(config.orbitalPrefab);
        var orbital = orbitalInstance.AddComponent<PlayerOrbital>();
        orbital.Initialize(player.transform, config.orbitRadius, config.rotationSpeed, config.damagePerSecond);

        // Escuchar cambios de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void DestroyOrbital()
    {
        if (orbitalInstance != null)
        {
            Destroy(orbitalInstance);
            orbitalInstance = null;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // chequea si la perk todavía está en los datos guardados
        var data = GameDataManager.Instance.playerData;
        bool perkActiva = data.initialPowerUps.Exists(p => p is OrbitalPowerUp);

        if (!perkActiva)
        {
            // si no está en la lista, destruye el orbital
            DestroyOrbital();
        }
        else
        {
            // si está, re-spawnea al entrar en la nueva escena
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            if (player != null && orbitalInstance == null)
            {
                SpawnOrbital(config, player);
            }
        }
    }
}