using UnityEngine;
using UnityEngine.UI;
using EasyUI.PickerWheelUI;

public class UpgradeWheel1 : MonoBehaviour
{
    [SerializeField] private PickerWheel picker;
    [SerializeField] private Button confirmarButton;

    private WheelPiece piezaGanadora;

    private void Start()
    {
        picker.OnSpinEnd += GuardarPremio;
        confirmarButton.onClick.AddListener(AplicarPremio);
    }

    private void GuardarPremio(WheelPiece pieza)
    {
        piezaGanadora = pieza;
        confirmarButton.interactable = true;
    }

    private void AplicarPremio()
    {
        if (piezaGanadora == null) return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("No se encontró el objeto Player con tag.");
            return;
        }

        switch (piezaGanadora.Index)
        {
            case 0:
                
                var health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.maxHealth = 4f;
                    health.currentHealth = 4f;
                    health.UpdateUI();
                }
                Debug.Log("✅ Vida máxima y actual aumentadas a 4");
                break;

            case 1:
                // Velocidad: 5 → 6.3
                var controller = player.GetComponent<PlayerController>();
                if (controller != null)
                {
                    controller.moveSpeed = 6.3f;
                }
                Debug.Log("✅ Velocidad aumentada a 6.3");
                break;

            case 2:
                // Damage de todos los comboAttacks a 15
                var attack = player.GetComponent<PlayerAttack>();
                if (attack != null && attack.comboAttacks != null)
                {
                    foreach (var a in attack.comboAttacks)
                        a.damage = 15f;
                }
                Debug.Log("✅ Daño de todos los ataques aumentado a 15");
                break;
        }

        confirmarButton.interactable = false;
        piezaGanadora = null;
    }
}
