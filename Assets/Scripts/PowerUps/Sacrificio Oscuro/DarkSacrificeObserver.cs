using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class DarkSacrificeObserver : MonoBehaviour
{
    [HideInInspector] public GameObject markerPrefab;
    [HideInInspector] public float spawnInterval = 6f;
    [HideInInspector] public float markerLifetime = 3f;
    [HideInInspector] public float yOffset = 0f;

    private PlayerController player;
    private float timer;
    private bool spawningEnabled = true;

    private GameObject activeMarker;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void BindPlayer(PlayerController pc) => player = pc;

    public void EnableSpawning(bool enable)
    {
        spawningEnabled = enable;
        timer = 0f;
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
       
        player = Object.FindFirstObjectByType<PlayerController>();
        
        activeMarker = null;
        
        timer = 0.25f;
    }

    private void Update()
    {
        if (!spawningEnabled || markerPrefab == null) return;

       
        if (player == null)
            player = Object.FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        
        if (activeMarker != null) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnMarkerUnderPlayer();
            timer = 0f;
        }
    }

    private void SpawnMarkerUnderPlayer()
    {
        if (player == null || markerPrefab == null) return;

        Vector3 pos = player.transform.position + new Vector3(0f, yOffset, 0f);
        activeMarker = Instantiate(markerPrefab, pos, Quaternion.identity);
        activeMarker.name = "DarkSacrificeMarker";
       
        Destroy(activeMarker, markerLifetime);
    }
}
