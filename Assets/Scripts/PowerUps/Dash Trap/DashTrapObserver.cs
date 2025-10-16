using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Evita error por timing
        StartCoroutine(ReassignPlayer());
    }

    private IEnumerator ReassignPlayer()
    {
        // Esperar hasta que el nuevo Player exista
        PlayerController found = null;
        while (found == null)
        {
            found = FindFirstObjectByType<PlayerController>();
            yield return null; // esperar un frame
        }

        player = found;
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