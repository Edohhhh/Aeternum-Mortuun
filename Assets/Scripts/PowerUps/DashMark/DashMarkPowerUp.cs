using UnityEngine;

[CreateAssetMenu(fileName = "DashMarkPowerUp", menuName = "PowerUps/Dash Mark Shield")]
public class DashMarkPowerUp : PowerUp
{
    [Header("Prefabs y tiempos")]
    public GameObject markPrefab;
    [Tooltip("Duraci�n del escudo al recoger la marca")]
    public float shieldDuration = 3f;
    [Tooltip("Duraci�n de la marca en el suelo")]
    public float markLifetime = 3f;

    public override void Apply(PlayerController player)
    {
        if (markPrefab == null)
        {
            Debug.LogWarning("[DashMark] Falta asignar markPrefab.");
            return;
        }

        // Crea (o reutiliza) un observer global
        var existing = GameObject.Find("DashMarkObserver");
        DashMarkObserver observer;

        if (existing == null)
        {
            var go = new GameObject("DashMarkObserver");
            observer = go.AddComponent<DashMarkObserver>();
            Object.DontDestroyOnLoad(go);
        }
        else
        {
            observer = existing.GetComponent<DashMarkObserver>();
        }

        observer.player = player;
        observer.markPrefab = markPrefab;
        observer.shieldDuration = shieldDuration;
        observer.markLifetime = markLifetime;
    }

    public override void Remove(PlayerController player)
    {
        // Si quer�s removerla por completo:
        var go = GameObject.Find("DashMarkObserver");
        if (go != null) Object.Destroy(go);
    }
}