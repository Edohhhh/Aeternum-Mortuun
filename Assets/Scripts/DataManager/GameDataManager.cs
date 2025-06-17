using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }
    public PlayerData playerData = new PlayerData();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // --- PRIMERA INICIALIZACIÓN: copiar valores por defecto del Player ---
            var initialPlayer = Object.FindFirstObjectByType<PlayerController>();
            if (initialPlayer != null)
            {
                // Esto volcará todas las stats según tu PlayerController/PlayerHealth actuales
                SavePlayerData(initialPlayer);
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
            player.LoadPlayerData();
    }

    public void SavePlayerData(PlayerController player)
    {
        // Movimiento / Dash
        playerData.moveSpeed = player.moveSpeed;
        playerData.dashSpeed = player.dashSpeed;
        playerData.dashIframes = player.dashIframes;
        playerData.dashSlideDuration = player.dashSlideDuration;
        playerData.dashDuration = player.dashDuration;
        playerData.dashCooldown = player.dashCooldown;

        // Salud
        var health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            playerData.maxHealth = health.maxHealth;
            playerData.currentHealth = health.currentHealth;
            playerData.regenerationRate = health.regenerationRate;
            playerData.regenDelay = health.regenDelay;
            playerData.invulnerableTime = health.invulnerableTime;
        }

        // Posición
        playerData.position = player.transform.position;
    }
}