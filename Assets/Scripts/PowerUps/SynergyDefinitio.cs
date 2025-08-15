using UnityEngine;

[CreateAssetMenu(fileName = "SynergyDefinition", menuName = "PowerUps/Synergy")]
public class SynergyDefinition : ScriptableObject
{
    public PowerUp perkA;
    public PowerUp perkB;
    public PowerUp result;
}

