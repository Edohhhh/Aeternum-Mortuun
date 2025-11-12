using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DashMark : MonoBehaviour
{
    private float lifetime = 3f;

    // Config del icono (inyectada por el Observer)
    private Sprite iconSprite;
    private Vector3 iconOffset = new Vector3(0f, 1.2f, 0f);
    private string iconSortingLayer = "";
    private int iconSortingOrder = 9999;
    private float iconBobAmplitude = 0.08f;
    private float iconBobSpeed = 3f;

    public void Initialize(float lifetime)
    {
        this.lifetime = lifetime;
    }

    public void ConfigureIcon(Sprite sprite, Vector3 offset, string sortingLayer, int sortingOrder, float bobAmp, float bobSpeed)
    {
        iconSprite = sprite;
        iconOffset = offset;
        iconSortingLayer = sortingLayer;
        iconSortingOrder = sortingOrder;
        iconBobAmplitude = bobAmp;
        iconBobSpeed = bobSpeed;
    }

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        // Añadir/obtener el ShieldGate del player
        var shield = pc.GetComponent<ShieldGate>();
        if (shield == null) shield = pc.gameObject.AddComponent<ShieldGate>();

        // Pasarle los datos del icono ANTES de activar
        shield.shieldIconSprite = iconSprite;
        shield.iconOffset = iconOffset;
        shield.iconBobAmplitude = iconBobAmplitude;
        shield.iconBobSpeed = iconBobSpeed;
        shield.iconSortingLayer = iconSortingLayer;
        shield.iconSortingOrder = iconSortingOrder;

        // Activar el escudo de un golpe (sin tiempo)
        shield.ActivateOneHit();

        Destroy(gameObject);
    }
}
