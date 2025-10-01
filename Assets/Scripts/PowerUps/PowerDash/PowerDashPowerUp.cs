using UnityEngine;

[CreateAssetMenu(fileName = "PowerDash", menuName = "PowerUps/Power Dash")]
public class PowerDash : PowerUp
{
    public GameObject dashMarkPrefab;

    public override void Apply(PlayerController player)
    {
        if (GameObject.Find("PowerDashObserver") != null) return;

        GameObject obj = new GameObject("PowerDashObserver");
        var observer = obj.AddComponent<PowerDashObserver>();
        observer.player = player;
        observer.dashMarkPrefab = dashMarkPrefab;

        Object.DontDestroyOnLoad(obj);
    }

    public override void Remove(PlayerController player) { }
}
