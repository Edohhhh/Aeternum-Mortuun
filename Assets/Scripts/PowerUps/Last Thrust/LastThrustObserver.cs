using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class LastThrustObserver : MonoBehaviour
{
    [Header("Bindings")]
    public PlayerController player;
    public GameObject shockwavePrefab;

    [Header("Combo")]
    [SerializeField] private int hitsNeeded = 3;
    [SerializeField] private float comboResetTime = 1.2f;

    private int comboCounter = 0;
    private float comboTimer = 0f;

    // Para no contar el mismo hitbox m�s de una vez
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
        // Primer enlace al player
        StartCoroutine(ReassignPlayer());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Al cambiar escena, limpiar cache y volver a enlazar
        countedHitboxes.Clear();
        comboCounter = 0;
        comboTimer = 0f;
        StartCoroutine(ReassignPlayer());
    }

    private IEnumerator ReassignPlayer()
    {
        PlayerController found = null;
        // Esperar un frame hasta que el nuevo Player exista en escena
        while (found == null)
        {
            found = FindFirstObjectByType<PlayerController>();
            yield return null;
        }
        player = found;
    }

    private void Update()
    {
        if (player == null) return;

        comboTimer -= Time.deltaTime;
        if (comboTimer <= 0f)
        {
            comboTimer = 0f;
            comboCounter = 0;
        }


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
            SpawnShockwave();
        }
    }

    private void SpawnShockwave()
    {
        if (shockwavePrefab == null || player == null) return;
        Instantiate(shockwavePrefab, player.transform.position, Quaternion.identity);
    }
}