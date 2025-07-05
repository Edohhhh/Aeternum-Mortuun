using UnityEngine;

[CreateAssetMenu(fileName = "New AcidTrailPowerUp", menuName = "PowerUps/Acid Trail")]
public class AcidTrailPowerUp : PowerUp
{
    public GameObject acidTrailPrefab;
    public float trailLifetime = 3f;

    public override void Apply(PlayerController player)
    {
        var dashTrail = player.GetComponent<AcidTrailDash>();
        if (dashTrail == null)
            dashTrail = player.gameObject.AddComponent<AcidTrailDash>();

        dashTrail.trailPrefab = acidTrailPrefab;
        dashTrail.trailDuration = trailLifetime;
        dashTrail.enabled = true;
    }

    public override void Remove(PlayerController player)
    {
        var dashTrail = player.GetComponent<AcidTrailDash>();
        if (dashTrail != null)
            dashTrail.enabled = false;
    }
}