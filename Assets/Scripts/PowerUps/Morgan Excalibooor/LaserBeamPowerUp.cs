using UnityEngine;

[CreateAssetMenu(fileName = "LaserBeamPowerUp", menuName = "PowerUps/Laser Beam")]
public class LaserBeamPowerUp : PowerUp
{
    [Header("Configuración del Laser")]
    public GameObject laserBeamPrefab;
    public float activationProbability = 0.1f; // 10%
    public float laserDuration = 4f;
    public float damagePerSecond = 10f;
    public float maxDistance = 15f;

    public override void Apply(PlayerController player)
    {
        // El observer se crea automáticamente con singleton
        // Solo aseguramos que exista
        if (LaserBeamObserver.Instance == null)
        {
            GameObject observerObj = new GameObject("LaserBeamObserver");
            var observer = observerObj.AddComponent<LaserBeamObserver>();
            observer.laserBeamPrefab = laserBeamPrefab;
            observer.activationProbability = activationProbability;
            observer.laserDuration = laserDuration;
            observer.damagePerSecond = damagePerSecond;
        }
        else
        {
            // Actualizar configuración
            var observer = LaserBeamObserver.Instance;
            observer.laserBeamPrefab = laserBeamPrefab;
            observer.activationProbability = activationProbability;
            observer.laserDuration = laserDuration;
            observer.damagePerSecond = damagePerSecond;
        }
    }

    public override void Remove(PlayerController player)
    {
        // No destruimos el observer para que pueda ser reutilizado
        // Solo desactivamos la funcionalidad si es necesario
    }
}