using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LifeUpPowerUp", menuName = "PowerUps/Life Up")]
public class LifeUpPowerUp : PowerUp
{
    public float extraMaxHealth = 0.5f;

    public override void Apply(PlayerController player)
    {
        if (GameObject.Find("LifeUpMarker") != null) return;

        // Crear marcador persistente
        GameObject marker = new GameObject("LifeUpMarker");
        Object.DontDestroyOnLoad(marker);

        // Aplicar el efecto luego de que PlayerHealth terminó su Start()
        player.StartCoroutine(ApplyDelayed(player, marker));
    }

    private IEnumerator ApplyDelayed(PlayerController player, GameObject marker)
    {
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

        // Actualizar la UI correctamente
        if (health.healthUI != null)
        {
            health.healthUI.Initialize(health.maxHealth);
            health.healthUI.UpdateHearts(health.currentHealth);
        }

        // PowerUp se borra de la lista
        var list = new List<PowerUp>(player.initialPowerUps);
        if (list.Contains(this))
        {
            list.Remove(this);
            player.initialPowerUps = list.ToArray();
        }


        marker.hideFlags = HideFlags.HideInHierarchy;
    }

    public override void Remove(PlayerController player) { }
}
