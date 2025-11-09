using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "HealthDownPowerUp", menuName = "PowerUps/Health Down (Permanent)")]
public class HealthDownPowerUp : PowerUp
{
    [Header("Cuánto resta a la vida máxima (en corazones, ej: 0.5 = medio corazón)")]
    public float reduceMaxHealth = 0.5f;

    [Header("Límite inferior para no dejar al jugador en 0")]
    public float minMaxHealth = 0.5f;

    public override void Apply(PlayerController player)
    {

        if (GameObject.Find("HealthDownMarker") != null) return;


        GameObject marker = new GameObject("HealthDownMarker");
        Object.DontDestroyOnLoad(marker);

        // qplicar el efecto luego de que PlayerHealth termine su Start()
        player.StartCoroutine(ApplyDelayed(player, marker));
    }

    private IEnumerator ApplyDelayed(PlayerController player, GameObject marker)
    {
        // espera un frame para asegurar que PlayerHealth y su UI estén inicializados
        yield return new WaitForEndOfFrame();

        var health = player.GetComponent<PlayerHealth>();
        if (health == null)
        {
            Object.Destroy(marker);
            yield break;
        }

        // calcular nueva vida máxima con límites
        float newMax = Mathf.Max(minMaxHealth, health.maxHealth - Mathf.Abs(reduceMaxHealth));


        if (Mathf.Approximately(newMax, health.maxHealth))
        {
            Object.Destroy(marker);
            yield break;
        }

        // aplicar reducción permanente
        health.maxHealth = newMax;


        health.currentHealth = Mathf.Min(health.currentHealth, health.maxHealth);


        if (health.healthUI != null)
        {
            health.healthUI.Initialize(health.maxHealth);
            health.healthUI.UpdateHearts(health.currentHealth);
        }

        // remover la perk de initialPowerUps
        var list = new List<PowerUp>(player.initialPowerUps);
        if (list.Contains(this))
        {
            list.Remove(this);
            player.initialPowerUps = list.ToArray();
        }

        // destruir el marker para permitir futuras aplicaciones
        Object.Destroy(marker);
    }

    public override void Remove(PlayerController player)
    {

    }
}