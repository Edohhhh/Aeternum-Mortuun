using UnityEngine;

[CreateAssetMenu(fileName = "ParryTimePowerUp", menuName = "PowerUps/ParryTime")]
public class ParryTimePowerUp : PowerUp
{
    [Header("Efectos")]
    public GameObject parryEffectPrefab;

    [Header("Parámetros")]
    public float parryWindow = 0.2f;
    public float invulnerability = 2f;
    public int damageAllOnParry = 3;
    public float bulletTimeScale = 0.25f;
    public float bulletTimeDuration = 1f;
    public float parryCooldown = 0.5f;

    public override void Apply(PlayerController player)
    {
        // Observer persistente (reaplica en nuevas escenas)
        var existing = GameObject.Find("ParryTimeObserver");
        ParryTimeObserver obs;
        if (existing == null)
        {
            var go = new GameObject("ParryTimeObserver");
            go.name = "ParryTimeObserver";
            obs = go.AddComponent<ParryTimeObserver>();
            Object.DontDestroyOnLoad(go);
        }
        else
        {
            obs = existing.GetComponent<ParryTimeObserver>();
        }

        // Config
        obs.parryEffectPrefab = parryEffectPrefab;
        obs.parryWindow = parryWindow;
        obs.invulnerability = invulnerability;
        obs.damageAllOnParry = damageAllOnParry;
        obs.bulletTimeScale = bulletTimeScale;
        obs.bulletTimeDuration = bulletTimeDuration;
        obs.parryCooldown = parryCooldown;

        // Atachar ya mismo (bloquea dash desde el primer frame)
        obs.AttachToPlayerNow(player);
    }

    public override void Remove(PlayerController player)
    {
        var go = GameObject.Find("ParryTimeObserver");
        if (go != null) Object.Destroy(go);
    }
}
