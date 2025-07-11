using UnityEngine;

public abstract class PowerUp : ScriptableObject
{
    [Header("Configuración")]
    public float duration = 5f;
    public bool isPermanent;

    [Header("Datos visuales")]
    public PowerUpEffect effect; // Referencia visual para la UI (icono, label)

    /// <summary>
    /// Aplica el efecto del power-up al jugador.
    /// </summary>
    public abstract void Apply(PlayerController player);

    /// <summary>
    /// Remueve el efecto del power-up del jugador.
    /// </summary>
    public abstract void Remove(PlayerController player);
}
