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

    private bool portalActivated = false;
    private bool playerInside = false;

    private void Reset()
    {
        // Asegura que el collider est? como trigger
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Update()
    {
        // 1) Activa portal cuando no quede ning?n "Enemy" en la escena
        if (!portalActivated && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            portalActivated = true;
        }

        // 2) Si est? activado, mu?velo hacia el punto
        if (portalActivated && targetPoint != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPoint.position,
                moveSpeed * Time.deltaTime
            );
        }

        // 3) Si el jugador est? dentro y pulsa F, cambia de escena
        if (playerInside && portalActivated && Input.GetKeyDown(KeyCode.F))
        {
            var player = Object.FindFirstObjectByType<PlayerController>();
            if (player != null)
                player.SavePlayerData();

            string nextScene = RoomRandomizer.Instance.GetNextRoom();

            if (!string.IsNullOrEmpty(nextScene))
            {
                if (ScreenFader.Instance != null)
                {
                    ScreenFader.Instance.FadeAndLoadScene(nextScene, 0.6f); // 0.6s por defecto, ajustalo
                }
                else
                {
                    // fallback si no existe el fader
                    SceneManager.LoadScene(nextScene);
                }
            }
            else
            {
                Debug.LogWarning("[PortalController] No quedan m?s salas en la run.");
            }
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
