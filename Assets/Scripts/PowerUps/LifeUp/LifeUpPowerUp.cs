using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LifeUpPowerUp", menuName = "PowerUps/Life Up")]
public class LifeUpPowerUp : PowerUp
{
    public float extraMaxHealth = 0.5f;

    public override void Apply(PlayerController player)
    {
        // Evitar doble aplicaci�n en el mismo frame/escena
        if (GameObject.Find("LifeUpMarker") != null) return;

        // Crear marcador persistente SOLO como guard temporal
        GameObject marker = new GameObject("LifeUpMarker");
        Object.DontDestroyOnLoad(marker);

        // Aplicar el efecto luego de que PlayerHealth termine su Start()
        player.StartCoroutine(ApplyDelayed(player, marker));
    }

    private IEnumerator ApplyDelayed(PlayerController player, GameObject marker)
    {
        // Espera un frame para asegurar que PlayerHealth y su UI est�n inicializados
        yield return new WaitForEndOfFrame();

        var health = player.GetComponent<PlayerHealth>();
        if (health == null)
        {
            Object.Destroy(marker);
            yield break;
        }

        // Aumentar salud real
        health.maxHealth += extraMaxHealth;
        health.currentHealth = Mathf.Min(health.currentHealth + extraMaxHealth, health.maxHealth);

        // Actualizar la UI correctamente (si la ten�s)
        if (health.healthUI != null)
        {
            health.healthUI.Initialize(health.maxHealth);
            health.healthUI.UpdateHearts(health.currentHealth);
        }

        // Remover la perk de initialPowerUps (si es de un solo uso al inicio)
        var list = new List<PowerUp>(player.initialPowerUps);
        if (list.Contains(this))
        {
            list.Remove(this);
            player.initialPowerUps = list.ToArray();
        }

        // Importante: destruir el marker para permitir futuras aplicaciones
        Object.Destroy(marker);
    }

    // Implementaci�n requerida por la clase base
    public override void Remove(PlayerController player)
    {
        // Intencionalmente vac�o: este power-up es de efecto permanente (one-shot).
        // Si quisieras revertirlo al quitar la perk, podr�as restar extraMaxHealth aqu� y actualizar la UI.
    }
}
