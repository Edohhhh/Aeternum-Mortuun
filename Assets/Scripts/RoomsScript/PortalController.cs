using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class PortalController : MonoBehaviour
{
    [Header("Destinations")]
    [Tooltip("Punto de destino al que debe moverse el portal")]
    [SerializeField] private Transform targetPoint;

    [Tooltip("Nombre de la escena a cargar al entrar y pulsar F")]
    [SerializeField] private string nextSceneName;

    [Header("Movement")]
    [Tooltip("Velocidad a la que el portal se desplaza hacia targetPoint")]
    [SerializeField] private float moveSpeed = 5f;

    private bool portalActivated = false;
    private bool playerInside = false;

    private void Reset()
    {
        // Asegura que el collider está como trigger
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Update()
    {
        // 1) Activa portal cuando no quede ningún "Enemy" en la escena
        if (!portalActivated && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            portalActivated = true;
        }

        // 2) Si está activado, muévelo hacia el punto
        if (portalActivated && targetPoint != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPoint.position,
                moveSpeed * Time.deltaTime
            );
        }

        // 3) Si el jugador está dentro y pulsa F, cambia de escena
        if (playerInside && portalActivated && Input.GetKeyDown(KeyCode.F))
        {
            var player = Object.FindFirstObjectByType<PlayerController>();
            if (player != null)
                player.SavePlayerData();


            if (!string.IsNullOrEmpty(nextSceneName))
                SceneManager.LoadScene(nextSceneName);
            else
                Debug.LogWarning("[PortalController] nextSceneName no está asignado.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }
}
