using UnityEngine;

[CreateAssetMenu(fileName = "SlowEnemiesOnPlayerHurt", menuName = "PowerUps/Slow Enemies On Player Hurt")]
public class SlowEnemiesOnPlayerHurtPowerUp : PowerUp
{
    [Header("Slow Config")]
    public float slowPercent = 0.1f;
    public float Powerduration = 2f;

    private GameObject observerInstance;

    public override void Apply(PlayerController player)
    {
        if (observerInstance != null) return;

        // Crear el observador como GameObject aparte 
        observerInstance = new GameObject("PlayerHurtSlowObserver");
        var observer = observerInstance.AddComponent<PlayerHurtSlowObserver>();
        observer.slowPercent = slowPercent;
        observer.duration = Powerduration;
        observer.AttachTo(player); // conecta el listener al player 
        Object.DontDestroyOnLoad(observerInstance);
    }

    public override void Remove(PlayerController player)
    {
        if (observerInstance != null)
        {
            Object.Destroy(observerInstance);
            observerInstance = null;
        }
    }
}