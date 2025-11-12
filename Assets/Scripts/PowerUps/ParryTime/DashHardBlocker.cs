using UnityEngine;

// Corre ANTES que la mayoría de scripts (incluyendo PlayerController) 
[DefaultExecutionOrder(-10000)]
[DisallowMultipleComponent]
public class DashHardBlocker : MonoBehaviour
{
    private PlayerController pc;
    private bool active;

    private void Awake()
    {
        pc = GetComponent<PlayerController>();
        // Bloquear desde el primer frame
        active = true;
        if (pc != null) pc.dashCooldownTimer = 999999f;
    }

    public void EnableBlocking(bool on)
    {
        active = on;
        if (pc != null && active)
            pc.dashCooldownTimer = 999999f;
    }

    private void OnEnable()
    {
        if (pc != null && active)
            pc.dashCooldownTimer = 999999f;
    }

    private void Update()
    {
        // Este Update corre antes que el PlayerController.Update
        if (active && pc != null)
        {
            // Mientras esté activa la perk, el cooldown NUNCA llega a 0
            pc.dashCooldownTimer = 999999f;
        }
    }

    private void OnDestroy()
    {
        // Si quitás la perk, podés permitir dash de nuevo:
        if (pc != null)
        {
            // liberar el cooldown (volverá a comportarse normal)
            pc.dashCooldownTimer = 0f;
        }
    }
}
