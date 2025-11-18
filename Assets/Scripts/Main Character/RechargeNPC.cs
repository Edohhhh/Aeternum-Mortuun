using UnityEngine;

public class RechargeNPC : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.F;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetKeyDown(interactKey))
            {
                // Recargar
                HealthDataNashe.Instance.healsLeft = HealthDataNashe.Instance.maxHeals;

                // Actualizar UI del jugador
                PlayerHealth ph = other.GetComponent<PlayerHealth>();
                if (ph != null)
                    ph.UpdateHealCounterUI();

                Debug.Log("Pociones recargadas por NPC!");
            }
        }
    }
}
