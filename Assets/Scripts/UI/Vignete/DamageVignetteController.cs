using UnityEngine;
using UnityEngine.UI;

public class DamageVignetteController : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;        // Asignar desde inspector
    public Image vignetteImage;             // Imagen del UI que hace el vignette

    [Header("Flash Settings")]
    public float flashAlpha = 0.65f;        // Opacidad cuando recibe daño
    public float flashDuration = 0.25f;     // Duración del flash

    [Header("Low HP Settings")]
    public float lowHPAlpha = 0.45f;        // Opacidad constante cuando tiene 1 HP
    public float lowHPThreshold = 1f;       // Vida mínima
    public float fadeSpeed = 5f;

    private float targetAlpha = 0f;
    private float lastHealth;

    private void Start()
    {
        if (playerHealth != null)
            lastHealth = playerHealth.currentHealth;
    }

    private void Update()
    {
        if (playerHealth == null || vignetteImage == null)
            return;

        // Detectar daño
        if (playerHealth.currentHealth < lastHealth)
        {
            TriggerDamageFlash();
        }

        // Si vida = 1 o menos → Vignette constante
        if (playerHealth.currentHealth <= lowHPThreshold && playerHealth.currentHealth > 0)
        {
            targetAlpha = lowHPAlpha;
        }
        else
        {
            // Si no está en 1hp y no está flasheando → se apaga
            if (targetAlpha != flashAlpha)
                targetAlpha = 0f;
        }

        // Fade de alpha
        Color c = vignetteImage.color;
        c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
        vignetteImage.color = c;

        // Guardar vida anterior
        lastHealth = playerHealth.currentHealth;
    }

    public void TriggerDamageFlash()
    {
        StopAllCoroutines();
        StartCoroutine(DamageFlashCoroutine());
    }

    private System.Collections.IEnumerator DamageFlashCoroutine()
    {
        targetAlpha = flashAlpha;

        yield return new WaitForSeconds(flashDuration);

        // Si está en 1 HP no se apaga después del flash
        if (playerHealth.currentHealth <= lowHPThreshold)
            targetAlpha = lowHPAlpha;
        else
            targetAlpha = 0f;
    }
}
