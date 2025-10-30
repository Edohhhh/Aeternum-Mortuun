using UnityEngine;
using System;
using System.Reflection;

public class ShieldGate : MonoBehaviour
{
    [Header("Escudo visual (opcional)")]
    [Tooltip("Prefab del c�rculo o aura que aparece mientras el escudo est� activo")]
    public GameObject shieldVisualPrefab;

    private GameObject visualInstance;

    [Header("Opcional: capa invulnerable")]
    public string invulnLayerName = "PlayerInvulnerable"; // si no existe, se ignora

    private PlayerHealth ph;
    private float timer;
    private bool active;

    // Snapshot
    private float lastHealth;

    // Reflection hooks
    private FieldInfo invFlagField;
    private PropertyInfo invFlagProp;
    private MethodInfo setInvMethod;
    private EventInfo onDamagedEvent;
    private Delegate onDamagedHandler;

    private int originalLayer = -1;
    private int invulnLayer = -1;

    private void Awake()
    {
        ph = GetComponent<PlayerHealth>();
        if (ph != null) lastHealth = ph.currentHealth;

        int layerId = LayerMask.NameToLayer(invulnLayerName);
        if (layerId >= 0) invulnLayer = layerId;

        if (ph != null)
        {
            var t = ph.GetType();
            invFlagField = t.GetField("invincible", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                         ?? t.GetField("isInvincible", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                         ?? t.GetField("invulnerable", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                         ?? t.GetField("iFrames", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            invFlagProp = t.GetProperty("Invincible", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                       ?? t.GetProperty("IsInvincible", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            setInvMethod = t.GetMethod("SetInvincible", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            onDamagedEvent = t.GetEvent("OnDamaged", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                          ?? t.GetEvent("DamageTaken", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }

    public void Activate(float duration)
    {
        timer = Mathf.Max(timer, duration);
        if (active) return;

        active = true;

        TrySetInvincible(true);
        TrySubscribeDamageEvent();

        if (invulnLayer >= 0)
        {
            originalLayer = gameObject.layer;
            gameObject.layer = invulnLayer;
        }

        if (ph != null) lastHealth = ph.currentHealth;

        // ?? Instanciar el prefab visual (c�rculo)
        if (shieldVisualPrefab != null)
        {
            visualInstance = Instantiate(shieldVisualPrefab, transform.position, Quaternion.identity, transform);
        }
    }

    private void Update()
    {
        if (!active) return;

        timer -= Time.deltaTime;

        if (ph != null && ph.currentHealth < lastHealth)
        {
            ph.currentHealth = lastHealth;
        }
        else if (ph != null)
        {
            lastHealth = ph.currentHealth;
        }

        if (timer <= 0f)
        {
            Deactivate();
        }
    }

    private void Deactivate()
    {
        if (!active) return;
        active = false;

        TrySetInvincible(false);
        TryUnsubscribeDamageEvent();

        if (originalLayer >= 0)
        {
            gameObject.layer = originalLayer;
            originalLayer = -1;
        }

        // ? Destruir el prefab visual
        if (visualInstance != null)
        {
            Destroy(visualInstance);
        }

        Destroy(this);
    }

    private void TrySetInvincible(bool val)
    {
        try
        {
            if (ph == null) return;

            if (invFlagField != null && invFlagField.FieldType == typeof(bool))
                invFlagField.SetValue(ph, val);

            if (invFlagProp != null && invFlagProp.PropertyType == typeof(bool) && invFlagProp.CanWrite)
                invFlagProp.SetValue(ph, val);

            if (setInvMethod != null)
                setInvMethod.Invoke(ph, new object[] { val });
        }
        catch { }
    }

    private void TrySubscribeDamageEvent()
    {
        try
        {
            if (ph == null || onDamagedEvent == null) return;
            onDamagedHandler = Delegate.CreateDelegate(
                onDamagedEvent.EventHandlerType,
                this,
                typeof(ShieldGate).GetMethod(nameof(OnPreDamageRelay), BindingFlags.NonPublic | BindingFlags.Instance));
            onDamagedEvent.AddEventHandler(ph, onDamagedHandler);
        }
        catch { }
    }

    private void TryUnsubscribeDamageEvent()
    {
        try
        {
            if (ph == null || onDamagedEvent == null || onDamagedHandler == null) return;
            onDamagedEvent.RemoveEventHandler(ph, onDamagedHandler);
            onDamagedHandler = null;
        }
        catch { }
    }

    private void OnPreDamageRelay(params object[] _)
    {
        if (ph != null) lastHealth = ph.currentHealth;
    }
}
