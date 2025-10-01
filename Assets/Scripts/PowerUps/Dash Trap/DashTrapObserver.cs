using UnityEngine;
using UnityEngine.SceneManagement;

public class DashTrapObserver : MonoBehaviour
{
    public PlayerController player;
    public GameObject trapPrefab;
    public float stunDuration = 2f;

    private int dashCount = 0;
    private float dashWindow = 1f;
    private float dashTimer = 0f;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Buscar al nuevo Player en la nueva escena
        player = FindFirstObjectByType<PlayerController>();
    }

    private void Update()
    {
        if (player == null) return;

        if (player.stateMachine.CurrentState == player.DashState)
        {
            dashTimer = dashWindow;
        }
        else
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                dashCount = 0;
            }
        }

        if (Input.GetButtonDown("Jump") && player.stateMachine.CurrentState == player.DashState)
        {
            dashCount++;

            if (dashCount >= 2)
            {
                SpawnTrap();
                dashCount = 0;
            }
        }
    }

    private void SpawnTrap()
    {
        Instantiate(trapPrefab, player.transform.position, Quaternion.identity);
    }
}