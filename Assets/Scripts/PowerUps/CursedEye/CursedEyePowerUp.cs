using UnityEngine;

[CreateAssetMenu(fileName = "CursedEyePowerUp", menuName = "PowerUps/Cursed Eye")]
public class CursedEyePowerUp : PowerUp
{
    public override void Apply(PlayerController player)
    {
        // Power up marcador para sinergias
        // No aplica efectos directos
    }

    public override void Remove(PlayerController player)
    {
        // No requiere limpieza
    }
}