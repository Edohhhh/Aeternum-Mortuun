using UnityEngine;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "BloodRitualPowerUp", menuName = "PowerUps/Blood Ritual")]
public class BloodRitualPowerUp : PowerUp
{
    [Range(0f, 1f)] public float healChance = 0.1f;
    public float healAmount = 0.5f;

    public override void Apply(PlayerController player)
    {
        // Singleton del observer
        var existing = GameObject.Find("BloodRitualObserver");
        BloodRitualObserver obs;

        if (existing == null)
        {
            var go = new GameObject("BloodRitualObserver");
            go.name = "BloodRitualObserver";
            obs = go.AddComponent<BloodRitualObserver>();
            Object.DontDestroyOnLoad(go);
        }
        else
        {
            obs = existing.GetComponent<BloodRitualObserver>();
        }

        // Config
        obs.healChance = healChance;
        obs.healAmount = healAmount;

        // Player actual (se re-enlaza solo en cada escena)
        obs.BindPlayer(player);
    }

    public override void Remove(PlayerController player)
    {
        var go = GameObject.Find("BloodRitualObserver");
        if (go != null) Object.Destroy(go);
    }
}
