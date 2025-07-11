using UnityEngine;

[System.Serializable]
public class PowerUpEntry
{
    public PowerUpEffect effect;
    [Range(0f, 100f)] public float chance = 100f;
}