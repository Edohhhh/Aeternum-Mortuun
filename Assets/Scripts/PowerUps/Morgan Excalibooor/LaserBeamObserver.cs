using UnityEngine;

public class LaserBeamObserver : MonoBehaviour
{
    public GameObject laserBeamPrefab;
    public float activationProbability = 0.1f; // 10%
    public float laserDuration = 4f;
    public float damagePerSecond = 10f;

    public static LaserBeamObserver Instance { get; private set; }

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
        // Solo activar en el tercer golpe
        if (comboIndex == 3)
        {
            float randomValue = Random.value;
            if (randomValue <= activationProbability)
            {
                ActivateLaserBeam(playerTransform);
            }
        }
    }

    private void ActivateLaserBeam(Transform player)
    {
        if (laserBeamPrefab == null || player == null) return;

        // Crear el laser beam
        GameObject laserObj = Instantiate(laserBeamPrefab, player.position, Quaternion.identity);
        LaserBeam laserBeam = laserObj.GetComponent<LaserBeam>();

        if (laserBeam == null)
        {
            laserBeam = laserObj.AddComponent<LaserBeam>();
        }

        // Configurar el laser
        laserBeam.duration = laserDuration;
        laserBeam.damagePerSecond = damagePerSecond;

        // Inicializar el laser para que siga al jugador
        laserBeam.Initialize(player);

        Debug.Log("[LASER BEAM] ¡Laser Beam activado! Duración: " + laserDuration + " segundos");
    }
}