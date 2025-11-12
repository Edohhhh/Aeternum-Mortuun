using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "DiceRNGPowerUp", menuName = "PowerUps/Lanza los dados!")]
public class DiceRNGPowerUp : PowerUp
{
    [Header("Dados (valores posibles)")]
    public int[] diceFaces = new int[] { -3, -2, -1, 1, 2, 3 };

    [Header("Overlay")]
    [Tooltip("Segundos de 'falso shuffle' por stat antes de mostrar el resultado")]
    public float rollAnimPerStat = 0.6f;

    [Tooltip("Segundos que el resultado final queda visible")]
    public float showFinalFor = 1.2f;

    public override void Apply(PlayerController player)
    {
        // Evita doble aplicación en el mismo frame/escena
        if (GameObject.Find("DiceRNGMarker") != null) return;

        // Marcador temporal para bloquear reprocesos
        var marker = new GameObject("DiceRNGMarker");
        Object.DontDestroyOnLoad(marker);

        // Overlay que tira los dados y aplica en escena actual
        var go = new GameObject("DiceRollOverlay");
        var overlay = go.AddComponent<DiceRollOverlay>();
        overlay.Initialize(player, diceFaces, rollAnimPerStat, showFinalFor, marker);

        // Remover la perk de initialPowerUps (one-shot)
        var list = new List<PowerUp>(player.initialPowerUps);
        if (list.Contains(this))
        {
            list.Remove(this);
            player.initialPowerUps = list.ToArray();
        }
    }

    public override void Remove(PlayerController player) { }
}
