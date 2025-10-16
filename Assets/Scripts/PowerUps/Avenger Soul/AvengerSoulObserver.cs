using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AvengerSoulObserver : MonoBehaviour
{
    public GameObject[] spiritPrefabs;
    public float delayAfterHit = 1f;

    private PlayerHealth playerHealth;
    private float lastKnownHealth;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        StartCoroutine(AssignPlayerHealth());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Volver a buscar el nuevo PlayerHealth cuando se carga otra escena
        StartCoroutine(AssignPlayerHealth());
    }

    private IEnumerator AssignPlayerHealth()
    {
        PlayerHealth found = null;

        // Esperar hasta que exista el PlayerHealth en la nueva escena
        while (found == null)
        {
            found = FindFirstObjectByType<PlayerHealth>();
            yield return null; // esperar un frame
        }

        playerHealth = found;
        lastKnownHealth = playerHealth.currentHealth;
    }

    private void Update()
    {
        if (playerHealth == null) return;

        // Detectar daño
        if (playerHealth.currentHealth < lastKnownHealth)
        {
            lastKnownHealth = playerHealth.currentHealth;
            StartCoroutine(SummonSpiritsWithDelay());
        }
        else
        {
            lastKnownHealth = playerHealth.currentHealth;
        }
    }

    private IEnumerator SummonSpiritsWithDelay()
    {
        yield return new WaitForSeconds(delayAfterHit);

        if (playerHealth == null) yield break;

        Vector3 basePos = playerHealth.transform.position;

        for (int i = 0; i < spiritPrefabs.Length; i++)
        {
            if (spiritPrefabs[i] == null) continue;

            Vector3 offset = new Vector3((i == 0 ? -0.5f : 0.5f), 0f, 0f);
            Instantiate(spiritPrefabs[i], basePos + offset, Quaternion.identity);
        }
    }
}
