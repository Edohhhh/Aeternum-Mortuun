using UnityEngine;

[CreateAssetMenu(fileName = "LastThrustPowerUp", menuName = "PowerUps/Last Thrust")]
public class LastThrustPowerUp : PowerUp
{
    public GameObject shockwavePrefab;

    public override void Apply(PlayerController player)
    {
        var existing = GameObject.Find("LastThrustObserver");
        LastThrustObserver observer;

        if (existing == null)
        {
            var go = new GameObject("LastThrustObserver");
            observer = go.AddComponent<LastThrustObserver>();
            Object.DontDestroyOnLoad(go);
        }
        else
        {
            observer = existing.GetComponent<LastThrustObserver>();
        }

        observer.shockwavePrefab = shockwavePrefab;
        observer.player = player; // se reasigna igual en sceneLoaded, pero esto lo habilita ya
    }

    public override void Remove(PlayerController player)
    {
        var go = GameObject.Find("LastThrustObserver");
        if (go != null) Object.Destroy(go);
    }
}
