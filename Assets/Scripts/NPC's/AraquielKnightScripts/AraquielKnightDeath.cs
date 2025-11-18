using UnityEngine;

public class AraquielKnightDeath : MonoBehaviour
{
    [Header("Configuración de Muerte")]
    public GameObject particulaDeSangrePrefab;
    public Vector3 rotacionDeParticula = new Vector3(0, 0, -90);

    [Header("Evento de UI")]
    public ShowUIOnDestroy uiWatcher; // <-- NUEVO

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            return;

        // Partícula
        if (particulaDeSangrePrefab != null)
        {
            Instantiate(particulaDeSangrePrefab, transform.position, Quaternion.Euler(rotacionDeParticula));
        }

        // Activa la UI si existe
        if (uiWatcher != null)
            uiWatcher.ActivateUI();

        // Se destruye
        Destroy(gameObject);
    }
}
