using UnityEngine;

[CreateAssetMenu(fileName = "ExtraSpinUp", menuName = "PowerUps/Extra Spin +1")]
public class ExtraSpinUp : PowerUp
{
    private void OnEnable()
    {
        // Marcamos que este power-up no expira y es permanente
        isPermanent = true;
    }

    public override void Apply(PlayerController player)
    {
        if (player == null) return;

        // Simplemente incrementa el contador.
        // El reseteo en LoadPlayerData() se encarga de que esto funcione
        // correctamente en cada carga de escena.
        player.extraSpins += 1;

        Debug.Log($"PowerUp aplicado: Tiradas extra ahora en {player.extraSpins}");
    }

    public override void Remove(PlayerController player)
    {
        // No hace nada, ya que es permanente
    }
}