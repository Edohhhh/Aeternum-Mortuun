using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DashMarkObserver : MonoBehaviour
{
    [Header("Bindings")]
    public PlayerController player;
    public GameObject markPrefab;

    [Header("Tiempos")]
    public float shieldDuration = 3f;
    public float markLifetime = 3f;

    [Header("Spawn aleatorio")]
    public float maxOffsetX = 0.6f;
    public float maxOffsetY = 0.4f;

    private bool wasDashing;
    private Vector3 dashStartPos;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Start() { StartCoroutine(ReassignPlayer()); }
    private void OnSceneLoaded(Scene s, LoadSceneMode m) { StartCoroutine(ReassignPlayer()); }

    private IEnumerator ReassignPlayer()
    {
        PlayerController found = null;
        while (found == null)
        {
            found = FindFirstObjectByType<PlayerController>();
            yield return null;
        }
        player = found;
        wasDashing = false;
    }

    private void Update()
    {
        if (player == null || markPrefab == null) return;

        bool isDashing = player.stateMachine != null && player.stateMachine.CurrentState == player.DashState;

        // Flanco de subida (acaba de empezar el dash): guardo la posici�n
        if (isDashing && !wasDashing)
        {
            dashStartPos = player.transform.position;
        }

        // Flanco de bajada (acaba de terminar el dash): spawneo la marca atr�s
        if (!isDashing && wasDashing)
        {
            Vector2 off = new(Random.Range(-maxOffsetX, maxOffsetX),
                              Random.Range(-maxOffsetY, maxOffsetY));
            SpawnMarkAt(dashStartPos + (Vector3)off);
        }

        wasDashing = isDashing;
    }

    private void SpawnMarkAt(Vector3 pos)
    {
        var mark = Instantiate(markPrefab, pos, Quaternion.identity);
        var comp = mark.GetComponent<DashMark>();
        if (comp == null) comp = mark.AddComponent<DashMark>();
        comp.Initialize(shieldDuration, markLifetime);
    }
}