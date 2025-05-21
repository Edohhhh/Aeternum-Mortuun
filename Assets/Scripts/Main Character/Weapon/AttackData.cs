using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Attack Data")]
public class AttackData : ScriptableObject
{
    public string attackName;
    public float damage;
    public float range;
    public float cooldown;
    public float hitboxRadius = 0.5f;
}
