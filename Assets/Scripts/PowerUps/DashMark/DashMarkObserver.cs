using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DashMarkObserver : MonoBehaviour
{
    [Header("Bindings")]
    public PlayerController player;
    public GameObject markPrefab;

    [Header("Marca")]
    public float markLifetime = 3f;

    [Header("Spawn aleatorio ligero alrededor del origen del dash")]
    public float maxOffsetX = 0.6f;
    public float maxOffsetY = 0.4f;

    [Header("Icono (inyectado desde la perk)")]
    public Sprite iconSprite;
    public Vector3 iconOffset = new Vector3(0f, 1.2f, 0f);
    public string iconSortingLayer = "";
    public int iconSortingOrder = 9999;
    public float iconBobAmplitude = 0.08f;
    public float iconBobSpeed = 3f;

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

        if (isDashing && !wasDashing)
            dashStartPos = player.transform.position;

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
        var go = Instantiate(markPrefab, pos, Quaternion.identity);
        var comp = go.GetComponent<DashMark>();
        if (comp == null) comp = go.AddComponent<DashMark>();

        // Inicializar duración de la marca
        comp.Initialize(markLifetime);

        // ► Pasar al DashMark la config del icono (que vino del ScriptableObject)
        comp.ConfigureIcon(iconSprite, iconOffset, iconSortingLayer, iconSortingOrder, iconBobAmplitude, iconBobSpeed);
    }
}
