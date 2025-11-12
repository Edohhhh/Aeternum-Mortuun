using UnityEngine;


public class ShieldGate : MonoBehaviour
{
    [Header("Icono sobre la cabeza")]
    public Sprite shieldIconSprite;          // Seteado por DashMark (viene del SO)
    public Vector3 iconOffset = new Vector3(0f, 1.2f, 0f);
    public float iconBobAmplitude = 0.08f;
    public float iconBobSpeed = 3f;

    [Tooltip("Opcional: Sorting Layer para el icono (vacío = ignorar)")]
    public string iconSortingLayer = "";
    public int iconSortingOrder = 9999;

    private GameObject iconInstance;
    private PlayerHealth ph;

    private bool active;
    private float lastHealth;

    private void Awake()
    {
        ph = GetComponent<PlayerHealth>();
        if (ph != null) lastHealth = ph.currentHealth;
    }

    public void ActivateOneHit()
    {
        if (active) return;
        active = true;

        if (ph != null) lastHealth = ph.currentHealth;

        if (shieldIconSprite != null && iconInstance == null)
        {
            iconInstance = new GameObject("ShieldIcon");
            var sr = iconInstance.AddComponent<SpriteRenderer>();
            sr.sprite = shieldIconSprite;

            if (!string.IsNullOrEmpty(iconSortingLayer))
                sr.sortingLayerName = iconSortingLayer;
            sr.sortingOrder = iconSortingOrder;

            var follow = iconInstance.AddComponent<ShieldIconFollow>();
            follow.target = transform;
            follow.offset = iconOffset;
            follow.bobAmplitude = iconBobAmplitude;
            follow.bobSpeed = iconBobSpeed;
        }
    }

    private void Update()
    {
        if (!active || ph == null) return;

        if (ph.currentHealth < lastHealth)
        {
            ph.currentHealth = lastHealth;

            if (ph.healthUI != null)
            {
                ph.healthUI.Initialize(ph.maxHealth);
                ph.healthUI.UpdateHearts(ph.currentHealth);
            }

            Deactivate();
            return;
        }

        lastHealth = ph.currentHealth;
    }

    private void Deactivate()
    {
        if (!active) return;
        active = false;

        if (iconInstance != null) Destroy(iconInstance);
        iconInstance = null;

        Destroy(this);
    }
}
