using UnityEngine;

[CreateAssetMenu(fileName = "DarkSacrificePowerUp", menuName = "PowerUps/Dark Sacrifice")] 
public class DarkSacrificePowerUp : PowerUp
{
    [Header("Configuraci�n de la marca")]
    public GameObject markerPrefab;

    [Tooltip("Capa que se considera obst�culo para evitar spawnear encima (ej: Walls, Ground)")] 
    public LayerMask obstacleMask;

    [Tooltip("Radio para verificar colisi�n con obst�culos")]
    public float checkRadius = 0.3f;

    [Tooltip("Distancia m�xima horizontal desde el jugador (izquierda/derecha)")]
    public float maxOffsetX = 5f;

    [Tooltip("Distancia m�xima vertical desde el jugador (arriba/abajo)")]
    public float maxOffsetY = 3f;

    public override void Apply(PlayerController player)
    {
        if (markerPrefab == null)
        {
            Debug.LogWarning("[DarkSacrifice] Prefab no asignado.");
            return;
        }

        // evitar duplicar el marcador 
        if (GameObject.Find("DarkSacrificeMarker") != null)
            return;

        // calcular una posici�n aleatoria cerca del jugador 
        Vector2 basePos = player.transform.position;
        Vector2 spawnPosition = GetValidRandomPosition(basePos);

        // instanciar la marca 
        GameObject marker = GameObject.Instantiate(markerPrefab, spawnPosition,
Quaternion.identity);
        marker.name = "DarkSacrificeMarker";
        GameObject.DontDestroyOnLoad(marker);
    }

    private Vector2 GetValidRandomPosition(Vector2 basePos)
    {
        Vector2 spawnPos = basePos + new Vector2(Random.Range(-maxOffsetX,
maxOffsetX),
                                                 Random.Range(-maxOffsetY, maxOffsetY));

        // hasta 8 intentos para encontrar un lugar libre 
        const int maxTries = 8;
        for (int i = 0; i < maxTries; i++)
        {
            if (!Physics2D.OverlapCircle(spawnPos, checkRadius, obstacleMask))
                break;

            spawnPos = basePos + new Vector2(Random.Range(-maxOffsetX, maxOffsetX),
                                             Random.Range(-maxOffsetY, maxOffsetY));
        }

        return spawnPos;
    }

    public override void Remove(PlayerController player)
    {
        // la marca se destruye sola o con el cambio de escena. 
    }
}