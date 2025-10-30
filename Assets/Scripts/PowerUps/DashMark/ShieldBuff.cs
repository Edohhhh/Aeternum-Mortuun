using UnityEngine;
using System.Collections;
using System.Reflection;

public class ShieldBuff : MonoBehaviour
{
    [Header("Visual")]
    public Color shieldColor = new Color(0.3f, 0.6f, 1f, 1f);
    private Color originalColor;
    private SpriteRenderer sr;

    // Duraci�n y estado
    private float timer;
    private bool active;

    // Salud
    private PlayerHealth playerHealth;
    private float lastHealth;

    // Reflexi�n (invencible nativo si existe)
    private FieldInfo invincibleField;
    private object healthHost;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        playerHealth = GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            lastHealth = playerHealth.currentHealth;

            // Buscar campos invencibles t�picos
            var t = playerHealth.GetType();
            invincibleField = t.GetField("invincible", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (invincibleField == null)
                invincibleField = t.GetField("isInvincible", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (invincibleField != null && invincibleField.FieldType == typeof(bool))
            {
                healthHost = playerHealth;
            }
        }
    }

    public void ApplyShield(float duration)
    {
        // activar / refrescar
        timer = duration;

        if (!active)
        {
            active = true;
            if (sr != null) { originalColor = sr.color; sr.color = shieldColor; }

            // activar invencibilidad nativa si existe
            if (invincibleField != null && healthHost != null)
            {
                invincibleField.SetValue(healthHost, true);
            }
        }
    }

    private void Update()
    {
        if (!active)
            return;

        timer -= Time.deltaTime;

        if (playerHealth != null)
        {
            // Fallback: si no hay invencibilidad nativa, restaurar salud si baj�
            if (invincibleField == null)
            {
                if (playerHealth.currentHealth < lastHealth)
                {
                    // Revertir el da�o sufrido mientras el escudo est� activo
                    playerHealth.currentHealth = lastHealth;
                }
                else
                {
                    lastHealth = playerHealth.currentHealth;
                }
            }
            else
            {
                // Mantener referencia del �ltimo valor por coherencia visual/log
                lastHealth = playerHealth.currentHealth;
            }
        }

        if (timer <= 0f)
        {
            Deactivate();
        }
    }

    private void Deactivate()
    {
        active = false;

        if (sr != null) sr.color = originalColor;

        // Apagar invencibilidad nativa
        if (invincibleField != null && healthHost != null)
        {
            invincibleField.SetValue(healthHost, false);
        }

        // Limpieza: como es un buff temporal, removemos el componente
        Destroy(this);
    }
}
