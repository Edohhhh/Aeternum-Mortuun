using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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

            var initialPlayer = Object.FindFirstObjectByType<PlayerController>();
            if (initialPlayer != null)
            {
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
        StartCoroutine(WaitAndLoadPlayer());
    }

    private System.Collections.IEnumerator WaitAndLoadPlayer()
    {
        yield return null; // esperar un frame

        var player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.LoadPlayerData();
        }
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

        playerData.baseDamage = player.baseDamage;

        // Salud
        var health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            playerData.maxHealth = (int)health.maxHealth;
            playerData.currentHealth = (int)health.currentHealth;
            playerData.regenerationRate = health.regenerationRate;
            playerData.regenDelay = health.regenDelay;
            playerData.invulnerableTime = health.invulnerableTime;
        }

        // Posición
        playerData.position = player.transform.position;

        // PowerUps (referencias directas)
        playerData.initialPowerUps.Clear();
        foreach (var powerUp in player.initialPowerUps)
        {
            if (powerUp != null)
                playerData.initialPowerUps.Add(powerUp);
        }
    }
}