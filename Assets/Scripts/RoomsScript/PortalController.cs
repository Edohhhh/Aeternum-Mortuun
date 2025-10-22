using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class PortalController : MonoBehaviour
{
    [Header("Destinations")]
    [Tooltip("Punto de destino al que debe moverse el portal")]
    [SerializeField] private Transform targetPoint;

    [Header("Movement")]
    [Tooltip("Velocidad a la que el portal se desplaza hacia targetPoint")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Settings")]
    [Tooltip("Si está activado, el portal comienza activo desde el inicio de la escena")]
    [SerializeField] private bool startActive = false;

    private bool portalActivated = false;
    private bool playerInside = false;

    private void Reset()
    {
        // Asegura que el collider esté como trigger
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Start()
    {
        // Si está marcado "Start Active", activamos el portal al inicio
        portalActivated = startActive;
        Debug.Log("[PortalController] Estado inicial del portal: " + portalActivated);
    }

    private void Update()
    {
        // 1) Activa el portal cuando no queden enemigos (solo si no es startActive)
        if (!portalActivated && !startActive && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            portalActivated = true;
            Debug.Log("[PortalController] Portal activado: no quedan enemigos.");
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
            Debug.Log("[PortalController] Jugador presionó F dentro del portal.");

            var player = Object.FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                try
                {
                    player.SavePlayerData();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("[PortalController] Error al guardar datos del jugador: " + e.Message);
                }
            }
            else
            {
                Debug.LogWarning("[PortalController] No se encontró el PlayerController en la escena.");
            }

            // Verificar que el RoomRandomizer esté presente
            if (RoomRandomizer.Instance == null)
            {
                Debug.LogWarning("[PortalController] No hay RoomRandomizer activo en la escena.");
                return;
            }

            string nextScene = RoomRandomizer.Instance.GetNextRoom();

            if (!string.IsNullOrEmpty(nextScene))
            {
                if (ScreenFader.Instance != null)
                {
                    ScreenFader.Instance.FadeAndLoadScene(nextScene, 0.6f);
                }
                else
                {
                    SceneManager.LoadScene(nextScene);
                }
            }
            else
            {
                Debug.LogWarning("[PortalController] No quedan más salas en la run.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            Debug.Log("[PortalController] Jugador entró al área del portal.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            Debug.Log("[PortalController] Jugador salió del área del portal.");
        }
    }
}
