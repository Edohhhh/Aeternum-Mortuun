using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ParryTimeObserver : MonoBehaviour
{
    [HideInInspector] public GameObject parryEffectPrefab;
    [HideInInspector] public float parryWindow = 0.2f;
    [HideInInspector] public float invulnerability = 2f;
    [HideInInspector] public int damageAllOnParry = 3;
    [HideInInspector] public float bulletTimeScale = 0.25f;
    [HideInInspector] public float bulletTimeDuration = 1f;
    [HideInInspector] public float parryCooldown = 0.5f;

    private PlayerController player;
    private ParryTimeRuntime runtime;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (runtime != null) Destroy(runtime);
    }

    public void AttachToPlayerNow(PlayerController pc)
    {
        player = pc;
        ApplyNow();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ReassignPlayer());
    }

    private IEnumerator ReassignPlayer()
    {
        PlayerController found = null;
        while (found == null)
        {
            found = Object.FindFirstObjectByType<PlayerController>();
            yield return null;
        }
        player = found;
        ApplyNow();
    }

    private void ApplyNow()
    {
        if (player == null) return;

        // Lógica del parry en el player
        if (runtime != null) Destroy(runtime);
        runtime = player.gameObject.AddComponent<ParryTimeRuntime>();
        runtime.Setup(parryEffectPrefab, parryWindow, invulnerability, damageAllOnParry,
                      bulletTimeScale, bulletTimeDuration, parryCooldown);

        // Asegurar el bloqueador de dash en este player
        var blocker = player.GetComponent<DashHardBlocker>();
        if (blocker == null) blocker = player.gameObject.AddComponent<DashHardBlocker>();
        blocker.EnableBlocking(true);
    }
}
