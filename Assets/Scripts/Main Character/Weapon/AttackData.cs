using UnityEngine;

[System.Serializable]
public class AttackData
{
    public string attackName;
    public string attackType; // <-- "stab", "swing", "cast", etc.
    public float damage;
    public float cooldown;
    public float range;
    public float hitboxRadius;
}
