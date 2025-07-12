using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryLoader : MonoBehaviour
{
    [Header("Scene Loading")]
    [Tooltip("Name of the scene to load when all enemies are defeated")]
    [SerializeField] private string nextSceneName;

    [Tooltip("Seconds to wait after defeating all enemies before loading the scene")]
    [SerializeField] private float delayBeforeLoad = 2f;

    private bool hasTriggered = false;

    private void Update()
    {
        // Si ya hemos iniciado la carga, no volvemos a entrar
        if (hasTriggered) return;

        // Comprobar cuántos objetos quedan con tag "Enemy"
        if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            hasTriggered = true;
            StartCoroutine(LoadVictoryScene());
        }
    }

    private IEnumerator LoadVictoryScene()
    {
        // Espera el tiempo configurado
        yield return new WaitForSeconds(delayBeforeLoad);

        // Carga la siguiente escena
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogWarning("[VictoryLoader] nextSceneName no está asignado.");
    }
}
