using UnityEngine;

public class LaserBeamChargeSystem : MonoBehaviour
{
    [Header("Configuración de Carga")]
    public float chargeRate = 0.1f; // Cantidad de carga por segundo
    public float dischargeRate = 0.05f; // Descarga cuando no se está cargando
    public float minChargeToFire = 0.3f; // 30% mínimo para disparar
    public float maxCharge = 1.0f; // 100% máximo

    [Header("Configuración del Laser")]
    public GameObject laserBeamPrefab;
    public float baseDamagePerSecond = 10f;
    public float baseMaxDistance = 15f;
    public float baseDuration = 4f;
    public float baseKnockbackForce = 5f;

    [Header("Visual de la Barra")]
    public GameObject chargeBarPrefab;
    public Vector2 barOffset = new Vector2(0, 1.5f); // Posición encima del jugador

    private float currentCharge = 0f;
    private bool isCharging = false;
    private PlayerController playerController;
    private GameObject chargeBarInstance;
    private ChargeBarController chargeBarController;
    private bool laserActive = false;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        CreateChargeBar();
    }

    private void Update()
    {
        if (laserActive) return; // No cargar mientras el laser está activo

        HandleInput();
        UpdateCharge();
        UpdateChargeBar();
    }

    private void HandleInput()
    {
        // Mantener presionado el botón para cargar (Fire2 por ejemplo)
        isCharging = Input.GetButton("Fire2"); // Puedes cambiar esto al botón que prefieras

        // Disparar cuando se suelta el botón y hay suficiente carga
        if (Input.GetButtonUp("Fire2") && currentCharge >= minChargeToFire)
        {
            FireLaser();
        }
    }

    private void UpdateCharge()
    {
        if (isCharging)
        {
            currentCharge += chargeRate * Time.deltaTime;
        }
        else
        {
            currentCharge -= dischargeRate * Time.deltaTime;
        }

        currentCharge = Mathf.Clamp(currentCharge, 0f, maxCharge);
    }

    private void UpdateChargeBar()
    {
        if (chargeBarController != null)
        {
            chargeBarController.UpdateCharge(currentCharge / maxCharge);
            chargeBarController.SetPosition(transform.position + (Vector3)barOffset);
        }
    }

    private void FireLaser()
    {
        if (laserBeamPrefab == null || currentCharge < minChargeToFire) return;

        // Calcular stats basados en la carga
        float chargePercentage = currentCharge / maxCharge;
        float damageMultiplier = Mathf.Lerp(1f, 2f, chargePercentage); // 1x a 2x daño
        float distanceMultiplier = Mathf.Lerp(1f, 1.5f, chargePercentage); // 1x a 1.5x distancia
        float knockbackMultiplier = Mathf.Lerp(1f, 1.5f, chargePercentage); // 1x a 1.5x knockback

        // Crear el laser beam
        GameObject laserObj = Instantiate(laserBeamPrefab, transform.position, Quaternion.identity);
        LaserBeam laserBeam = laserObj.GetComponent<LaserBeam>();

        if (laserBeam == null)
        {
            laserBeam = laserObj.AddComponent<LaserBeam>();
        }

        // Configurar el laser con stats escalados
        laserBeam.damagePerSecond = baseDamagePerSecond * damageMultiplier;
        laserBeam.maxDistance = baseMaxDistance * distanceMultiplier;
        laserBeam.duration = baseDuration; // Duración fija
        laserBeam.knockbackForce = baseKnockbackForce * knockbackMultiplier;

        // Inicializar el laser para que siga al jugador
        laserBeam.Initialize(transform);

        // Resetear carga
        currentCharge = 0f;
        laserActive = true;

        // Desactivar el laser activo después de la duración
        Invoke(nameof(DeactivateLaser), baseDuration);

        Debug.Log($"[LASER BEAM] ¡Laser Beam disparado! Carga: {chargePercentage:P0}, Daño: {laserBeam.damagePerSecond}, Distancia: {laserBeam.maxDistance}");
    }

    private void DeactivateLaser()
    {
        laserActive = false;
    }

    private void CreateChargeBar()
    {
        if (chargeBarPrefab != null)
        {
            chargeBarInstance = Instantiate(chargeBarPrefab);
            chargeBarController = chargeBarInstance.GetComponent<ChargeBarController>();
        }
        else
        {
            // Crear barra básica si no hay prefab
            CreateBasicChargeBar();
        }
    }

    private void CreateBasicChargeBar()
    {
        chargeBarInstance = new GameObject("ChargeBar");
        chargeBarInstance.transform.SetParent(transform);

        GameObject background = new GameObject("Background");
        background.transform.SetParent(chargeBarInstance.transform);
        SpriteRenderer bgRenderer = background.AddComponent<SpriteRenderer>();
        bgRenderer.color = Color.black;
        bgRenderer.sortingOrder = 10;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(chargeBarInstance.transform);
        SpriteRenderer fillRenderer = fill.AddComponent<SpriteRenderer>();
        fillRenderer.color = Color.red;
        fillRenderer.sortingOrder = 11;

        chargeBarController = chargeBarInstance.AddComponent<ChargeBarController>();
        chargeBarController.Initialize(fillRenderer, bgRenderer);
    }

    private void OnDestroy()
    {
        if (chargeBarInstance != null)
        {
            Destroy(chargeBarInstance);
        }
    }
}