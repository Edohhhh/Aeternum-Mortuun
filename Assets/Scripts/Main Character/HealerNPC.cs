using UnityEngine;

public class HealerNPC : MonoBehaviour
{
    [Header("Configuración del NPC Sanador")]
    public KeyCode interactKey = KeyCode.F;
    private bool playerInRange = false;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            HealPlayerPotions();
        }
    }

    private void HealPlayerPotions()
    {
        // Usa el sistema real de almacenamiento persistente
        HealthDataNashe.Instance.healsLeft = HealthDataNashe.Instance.maxHeals;

        // Actualizar UI del jugador
        PlayerHealth player = FindObjectOfType<PlayerHealth>();
        if (player != null)
            player.UpdateHealCounterUI();

        Debug.Log("🟢 NPC sanador recargó todas tus curas.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}

