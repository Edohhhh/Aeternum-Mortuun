using UnityEngine;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "DashMarkPowerUp", menuName = "PowerUps/Dash Mark Shield")]
public class DashMarkPowerUp : PowerUp
{
    [Header("Prefab de la marca")]
    public GameObject markPrefab;

    [Header("Marca")]
    [Tooltip("Duración de la marca en el suelo (s)")]
    public float markLifetime = 3f;

    [Header("Icono del escudo (desde la perk)")]
    [Tooltip("Sprite que se mostrará arriba del jugador mientras el escudo esté activo")]
    public Sprite shieldIconSprite;
    public Vector3 iconOffset = new Vector3(0f, 1.2f, 0f);
    [Tooltip("Sorting Layer opcional (vacío = usar la actual)")]
    public string iconSortingLayer = "";
    public int iconSortingOrder = 9999;
    public float iconBobAmplitude = 0.08f;
    public float iconBobSpeed = 3f;

    public override void Apply(PlayerController player)
    {
        if (markPrefab == null)
        {
            Debug.LogWarning("[DashMark] Falta asignar markPrefab.");
            return;
        }

        var existing = GameObject.Find("DashMarkObserver");
        DashMarkObserver observer;

        if (existing == null)
        {
            var go = new GameObject("DashMarkObserver");
            observer = go.AddComponent<DashMarkObserver>();
            go.name = "DashMarkObserver";
            Object.DontDestroyOnLoad(go);
        }
        else
        {
            observer = existing.GetComponent<DashMarkObserver>();
        }

        // Bindings
        observer.player = player;
        observer.markPrefab = markPrefab;
        observer.markLifetime = markLifetime;

        // ► Config del icono tomada DEL SO
        observer.iconSprite = shieldIconSprite;
        observer.iconOffset = iconOffset;
        observer.iconSortingLayer = iconSortingLayer;
        observer.iconSortingOrder = iconSortingOrder;
        observer.iconBobAmplitude = iconBobAmplitude;
        observer.iconBobSpeed = iconBobSpeed;
    }

    public override void Remove(PlayerController player)
    {
        var go = GameObject.Find("DashMarkObserver");
        if (go != null) Object.Destroy(go);
    }
}
