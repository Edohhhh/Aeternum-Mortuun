using UnityEngine;

[CreateAssetMenu(fileName = "DashTrapPowerUp", menuName = "PowerUps/Dash Trap")]
public class DashTrapPowerUp : PowerUp
{
    public GameObject trapPrefab;
    public float stunDuration = 2f;

    public override void Apply(PlayerController player)
    {
        if (GameObject.Find("DashTrapObserver") != null) return;

        var go = new GameObject("DashTrapObserver");
        var observer = go.AddComponent<DashTrapObserver>();
        observer.player = player;
        observer.trapPrefab = trapPrefab;
        observer.stunDuration = stunDuration;

        Object.DontDestroyOnLoad(go);
    }

    public override void Remove(PlayerController player)
    {
        var obs = GameObject.Find("DashTrapObserver");
        if (obs != null) Object.Destroy(obs);
    }
}
