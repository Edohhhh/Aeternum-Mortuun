using UnityEngine;

public abstract class PowerUp : ScriptableObject
{
    [Header("Configuraci�n")]
    public float duration = 5f;
    public bool isPermanent;

    public abstract void Apply(PlayerController player);
    public abstract void Remove(PlayerController player);
}