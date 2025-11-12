using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class GamblingKnightObserver : MonoBehaviour
{
    [HideInInspector] public GameObject slotMachinePrefab;
    [HideInInspector] public float spawnBehindDistance = 1.6f;
    [HideInInspector] public float rollCooldown = 10f;
    [HideInInspector] public List<WeightedOutcome> outcomes = new();

    private GameObject machineInstance;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (machineInstance != null) Destroy(machineInstance);
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // limpiar efectos temporales al entrar a cada escena
        GamblerEffects.CleanupSceneTemp();
        SpawnForCurrentScene();
    }

    public void SpawnForCurrentScene()
    {
        if (slotMachinePrefab == null) return;

        if (machineInstance != null) Destroy(machineInstance);

        var player = Object.FindFirstObjectByType<PlayerController>();
        Vector3 pos = Vector3.zero;

        if (player != null)
        {
            Vector2 back = player.lastNonZeroMoveInput.sqrMagnitude > 0.0001f
                ? -player.lastNonZeroMoveInput.normalized
                : Vector2.left;

            pos = player.transform.position + (Vector3)(back * spawnBehindDistance);
        }

        machineInstance = Instantiate(slotMachinePrefab, pos, Quaternion.identity);
        machineInstance.name = "SlotMachine_GK";
        var sm = machineInstance.GetComponent<SlotMachineBehaviour>();
        if (sm == null) sm = machineInstance.AddComponent<SlotMachineBehaviour>();

        sm.Setup(this, rollCooldown, outcomes);
    }
}
