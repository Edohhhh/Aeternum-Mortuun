using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynergyResolver : MonoBehaviour
{
    public List<SynergyDefinition> synergies;

    private void OnEnable()
    {
        DontDestroyOnLoad(gameObject);
        StartCoroutine(WaitAndCheck());
    }

    private IEnumerator WaitAndCheck()
    {
        // PlayerController termine de aplicar efectos perks
        yield return new WaitForSeconds(0.1f);

        var player = Object.FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            Debug.Log("[SYNERGY] Resolviendo sinergias...");
            CheckAndApplySynergies(player);
        }
    }

    public void CheckAndApplySynergies(PlayerController player)
    {
        var current = new List<PowerUp>(player.initialPowerUps);
        bool modified = false;

        foreach (var synergy in synergies)
        {
            if (current.Contains(synergy.perkA) && current.Contains(synergy.perkB))
            {

                // eliminar efectos activos de A y B
                synergy.perkA.Remove(player);
                synergy.perkB.Remove(player);

                // Que se saque de la lista y deje de apicar los efecrtso
                current.Remove(synergy.perkA);
                current.Remove(synergy.perkB);

                // Agregar perk sinergiada
                current.Add(synergy.result);
                synergy.result.Apply(player);

                modified = true;
            }
        }

        if (modified)
        {
            player.initialPowerUps = current.ToArray();
        }
    }
}