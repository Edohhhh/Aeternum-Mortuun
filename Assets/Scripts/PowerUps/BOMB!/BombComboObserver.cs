using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class BombComboObserver : MonoBehaviour
{
    [Header("Bindings")]
    public PlayerController player;
    public GameObject bombPrefab;

    [Header("Lógica de combo")]
    [Range(0f, 1f)] public float spawnChance = 0.1f;
    [SerializeField] private int hitsNeeded = 3;
    [SerializeField] private float comboResetTime = 1.2f;

    private int comboCounter = 0;
    private float comboTimer = 0f;

    // Evita contar el mismo hitbox dos veces
    private readonly HashSet<int> countedHitboxes = new HashSet<int>();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        StartCoroutine(ReassignPlayer());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset combo y cache al cambiar de escena
        countedHitboxes.Clear();
        comboCounter = 0;
        comboTimer = 0f;
        StartCoroutine(ReassignPlayer());
    }

    private IEnumerator ReassignPlayer()
    {
        PlayerController found = null;
        while (found == null)
        {
            found = FindFirstObjectByType<PlayerController>();
            yield return null; // esperar un frame hasta que el Player exista
        }
        player = found;
    }

    private void Update()
    {
        if (player == null) return;

        // Ventana de combo
        comboTimer -= Time.deltaTime;
        if (comboTimer <= 0f)
        {
            comboTimer = 0f;
            comboCounter = 0;
        }

        // Buscar SOLO hitboxes bajo el player (no en toda la escena)
        var hitboxes = player.GetComponentsInChildren<AttackHitbox>(true);
        for (int i = 0; i < hitboxes.Length; i++)
        {
            var hb = hitboxes[i];
            if (hb == null) continue;

            int id = hb.GetInstanceID();
            if (!countedHitboxes.Contains(id))
            {
                countedHitboxes.Add(id);
                RegisterHit();
            }
        }
    }

    private void RegisterHit()
    {
        comboCounter++;
        comboTimer = comboResetTime;

        if (comboCounter >= hitsNeeded)
        {
            comboCounter = 0;
            if (bombPrefab != null && Random.value <= spawnChance)
            {
                SpawnBomb();
            }
        }
    }

    private void SpawnBomb()
    {
        if (player == null) return;
        Instantiate(bombPrefab, player.transform.position, Quaternion.identity);
    }
}
