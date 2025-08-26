using UnityEngine;

[CreateAssetMenu(fileName = "ChargedLaserPowerUp", menuName = "PowerUps/Charged Laser Beam")]
public class ChargedLaserPowerUp : PowerUp
{
    [Header("Configuración de Carga")]
    public float chargeRate = 0.1f;
    public float dischargeRate = 0.05f;

    [Header("Configuración del Laser")]
    public GameObject laserBeamPrefab;
    public float baseDamagePerSecond = 10f;
    public float baseMaxDistance = 15f;
    public float baseDuration = 4f;
    public float baseKnockbackForce = 5f;

    [Header("Visual de la Barra")]
    public GameObject chargeBarPrefab;

    public override void Apply(PlayerController player)
    {
        var chargeSystem = player.GetComponent<LaserBeamChargeSystem>();
        if (chargeSystem == null)
        {
            chargeSystem = player.gameObject.AddComponent<LaserBeamChargeSystem>();
        }

        // Configurar el sistema de carga
        chargeSystem.chargeRate = chargeRate;
        chargeSystem.dischargeRate = dischargeRate;
        chargeSystem.laserBeamPrefab = laserBeamPrefab;
        chargeSystem.baseDamagePerSecond = baseDamagePerSecond;
        chargeSystem.baseMaxDistance = baseMaxDistance;
        chargeSystem.baseDuration = baseDuration;
        chargeSystem.baseKnockbackForce = baseKnockbackForce;
        chargeSystem.chargeBarPrefab = chargeBarPrefab;

        chargeSystem.enabled = true;
    }

    public override void Remove(PlayerController player)
    {
        var chargeSystem = player.GetComponent<LaserBeamChargeSystem>();
        if (chargeSystem != null)
        {
            chargeSystem.enabled = false;
        }
    }
}