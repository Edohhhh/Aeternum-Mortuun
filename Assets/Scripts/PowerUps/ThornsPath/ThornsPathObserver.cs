using UnityEngine;

public class ThornsPathObserver : MonoBehaviour
{
    public GameObject thornsPathPrefab;
    public float activationProbability = 0.1f; // 10%
    public float thornsPathDuration = 4f;
    public float damagePerSecond = 10f;

    public static ThornsPathObserver Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnComboCompleted(int comboIndex, Transform playerTransform)
    {
        if (comboIndex == 3)
        {
            float randomValue = Random.value;
            if (randomValue <= activationProbability)
            {
                ActivateThornsPath(playerTransform);
            }
        }
    }

    private void ActivateThornsPath(Transform player)
    {
        if (thornsPathPrefab == null || player == null) return;

        // Obtener posición del mouse en world space
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = player.position.z; // Mantener la misma profundidad

        // Calcular dirección desde el jugador al mouse
        Vector2 startPosition = player.position;
        Vector2 direction = (mousePosition - player.position).normalized;

        // Calcular la distancia entre jugador y mouse
        float distance = Vector2.Distance(player.position, mousePosition);

        // Calcular posición final basada en la dirección y distancia
        Vector2 endPosition = startPosition + direction * distance;

        GameObject thornsObj = Instantiate(thornsPathPrefab, player.position, Quaternion.identity);
        ThornsPath thornsPath = thornsObj.GetComponent<ThornsPath>();
        if (thornsPath == null)
        {
            thornsPath = thornsObj.AddComponent<ThornsPath>();
        }

        thornsPath.duration = thornsPathDuration;
        thornsPath.damagePerSecond = damagePerSecond;
        thornsPath.Initialize(startPosition, endPosition);

        Debug.Log("[THORNS PATH] ¡Thorns Path activado! Duración: " + thornsPathDuration + " segundos");
    }
}