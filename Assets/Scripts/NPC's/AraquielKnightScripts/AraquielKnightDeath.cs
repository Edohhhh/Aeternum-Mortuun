using UnityEngine;

// El nombre del archivo DEBE ser AraquielKnightDeath.cs
public class AraquielKnightDeath : MonoBehaviour
{
    [Header("Configuración de Muerte")]

    [Tooltip("El Prefab de la partícula de sangre que se creará.")]
    public GameObject particulaDeSangrePrefab;

    [Tooltip("Rotación de la partícula. Para 'Dirección X' (derecha), usa (0, 0, -90).")]
    public Vector3 rotacionDeParticula = new Vector3(0, 0, -90);

    // Esta función se ejecuta cuando CUALQUIER otro Collider2D entra en el trigger de este objeto.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 🚨 NUEVA COMPROBACIÓN:
        // Si el objeto que entra en el trigger tiene el Tag "Player", salimos de la función
        // y no hacemos NADA (no se destruye, no se crean partículas).
        if (other.CompareTag("Player"))
        {
            return;
        }

        // Si la ejecución llega aquí, significa que la colisión NO es con el jugador.

        // 1. Instanciar la partícula de sangre (si hay una asignada)
        if (particulaDeSangrePrefab != null)
        {
            Quaternion rotacion = Quaternion.Euler(rotacionDeParticula);

            // Creamos la partícula en la posición de ESTE objeto
            Instantiate(particulaDeSangrePrefab, transform.position, rotacion);
        }

        // 2. Destruir ESTE GameObject (el objeto que tiene este script)
        Destroy(gameObject);
    }
}