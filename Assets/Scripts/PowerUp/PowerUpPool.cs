using UnityEngine;

[CreateAssetMenu(menuName = "Ruleta/PowerUpPool")]
public class PowerUpPool : ScriptableObject
{
    public PowerUpEntry[] entries;

    public int Count => entries?.Length ?? 0;
    public PowerUpEffect GetEffect(int index) => entries[index].effect;
    public float GetChance(int index) => entries[index].chance;
}