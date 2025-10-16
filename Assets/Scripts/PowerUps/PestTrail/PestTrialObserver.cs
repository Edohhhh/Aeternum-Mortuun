using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PestTrailObserver : MonoBehaviour
{
    private Transform player;
    private GameObject acidCloudPrefab;
    private float interval;
    private float timer = 0f;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Initialize(Transform playerTransform, GameObject cloudPrefab, float spawnInterval)
    {
        player = playerTransform;
        acidCloudPrefab = cloudPrefab;
        interval = spawnInterval;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ReassignPlayer());
    }

    private IEnumerator ReassignPlayer()
    {
        // Esperar hasta que el nuevo Player exista en la escena
        PlayerController found = null;
        while (found == null)
        {
            found = FindFirstObjectByType<PlayerController>();
            yield return null; // espera 1 frame
        }

        player = found.transform;
    }

    private void Update()
    {
        if (player == null) return;

        Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (move != Vector2.zero)
        {
            timer += Time.deltaTime;
            if (timer >= interval)
            {
                GameObject cloud = Instantiate(acidCloudPrefab, player.position, Quaternion.identity);
                timer = 0f;
            }
        }
    }
}