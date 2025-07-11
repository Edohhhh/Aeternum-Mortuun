using UnityEngine;

public class AcidTrailDash : MonoBehaviour
{
    public GameObject trailPrefab;
    public float trailDuration = 3f;

    private PlayerController player;
    private float spawnCooldown = 0.05f;
    private float timer;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (player == null || player.stateMachine.CurrentState != player.DashState)
            return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnTrail();
            timer = spawnCooldown;
        }
    }

    private void SpawnTrail()
    {
        if (trailPrefab == null) return;
        var trail = Instantiate(trailPrefab, transform.position, Quaternion.identity);
        Destroy(trail, trailDuration);
    }
}