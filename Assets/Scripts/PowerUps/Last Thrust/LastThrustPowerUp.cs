using UnityEngine;

[CreateAssetMenu(fileName = "LastThrustPowerUp", menuName = "PowerUps/Last Thrust")]
public class LastThrustPowerUp : PowerUp
{
    public GameObject shockwavePrefab;

    public override void Apply(PlayerController player)
    {
        if (GameObject.Find("LastThrustObserver") != null) return;

        var go = new GameObject("LastThrustObserver");
        var observer = go.AddComponent<LastThrustObserver>();
        observer.player = player;
        observer.shockwavePrefab = shockwavePrefab;

        Object.DontDestroyOnLoad(go);
    }

    public override void Remove(PlayerController player)
    {
        var go = GameObject.Find("LastThrustObserver");
        if (go != null) Destroy(go);
    }
}
