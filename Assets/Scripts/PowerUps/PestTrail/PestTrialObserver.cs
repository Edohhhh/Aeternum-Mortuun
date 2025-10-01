using UnityEngine;

public class PestTrailObserver : MonoBehaviour
{
    private Transform player;
    private GameObject acidCloudPrefab;
    private float interval;

    private float timer = 0f;

    public void Initialize(Transform playerTransform, GameObject cloudPrefab, float spawnInterval)
    {
        player = playerTransform;
        acidCloudPrefab = cloudPrefab;
        interval = spawnInterval;
    }

    void Update()
    {
        if (player == null) return;

        Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (move != Vector2.zero)
        {
            timer += Time.deltaTime;
            if (timer >= interval)
            {
                GameObject cloud = GameObject.Instantiate(acidCloudPrefab, player.position, Quaternion.identity);
                timer = 0f;
            }
        }
    }
}