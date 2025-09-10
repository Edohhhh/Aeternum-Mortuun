using UnityEngine;

[CreateAssetMenu(fileName = "WakeyWakey", menuName = "PowerUps/Wakey Wakey")]
public class WakeyWakeyPowerUp : PowerUp
{
    public GameObject knightPrefab;
    public int attackDamage = 5;

    private GameObject instance;

    public override void Apply(PlayerController player)
    {
        if (GameObject.Find("WakeyWakeyKnight") != null) return;

        instance = GameObject.Instantiate(knightPrefab);
        instance.name = "WakeyWakeyKnight";

        var logic = instance.AddComponent<WakeyKnight>();
        logic.Initialize(player.transform, attackDamage);

        Object.DontDestroyOnLoad(instance);
    }

    public override void Remove(PlayerController player)
    {
        if (instance != null)
            Object.Destroy(instance);
    }
}
