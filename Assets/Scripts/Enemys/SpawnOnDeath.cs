using UnityEngine;

public class SpawnOnDeath : MonoBehaviour
{
    [Header("Collider a vigilar")]
    public Collider2D targetCollider;

    [Header("Portal a activar")]
    public GameObject portalToActivate;

    private bool portalActivated = false;

    private void Update()
    {
        // Evita activarlo más de una vez
        if (portalActivated) return;

        // Si el collider existe y está deshabilitado
        if (targetCollider == null || !targetCollider.enabled || !targetCollider.gameObject.activeInHierarchy)
        {
            ActivatePortal();
        }
    }

    private void ActivatePortal()
    {
        if (portalToActivate != null)
        {
            portalToActivate.SetActive(true);
        }

        portalActivated = true;
    }
}