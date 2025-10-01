using UnityEngine;

[CreateAssetMenu(fileName = "DarkSacrificePowerUp", menuName = "PowerUps/Dark Sacrifice")]
public class DarkSacrificePowerUp : PowerUp
{
    public GameObject markerPrefab;

    public override void Apply(PlayerController player)
    {
        if (markerPrefab == null)
        {
            Debug.LogWarning("[DarkSacrifice] Prefab no asignado.");
            return;
        }

        if (GameObject.Find("DarkSacrificeMarker") != null)
            return;

        // Spawn del marcador
        Vector2 spawnPosition = player.transform.position + new Vector3(2, 0, 0);
        GameObject marker = GameObject.Instantiate(markerPrefab, spawnPosition, Quaternion.identity);
        marker.name = "DarkSacrificeMarker";
        GameObject.DontDestroyOnLoad(marker);
    }

    public override void Remove(PlayerController player) { }
}
