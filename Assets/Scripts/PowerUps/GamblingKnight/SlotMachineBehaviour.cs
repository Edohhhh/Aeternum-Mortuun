using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class SlotMachineBehaviour : MonoBehaviour
{
    private float cooldown = 10f;
    private float nextUseTime = 0f;

    private List<WeightedOutcome> outcomes;
    private GamblingKnightObserver owner;

    private bool playerInside;
    private PlayerController player;

    public void Setup(GamblingKnightObserver owner, float cd, List<WeightedOutcome> outs)
    {
        this.owner = owner;
        cooldown = cd;
        outcomes = new List<WeightedOutcome>(outs);

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc != null)
        {
            playerInside = true;
            player = pc;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc != null && pc == player)
        {
            playerInside = false;
            player = null;
        }
    }

    private void Update()
    {
        if (!playerInside || player == null) return;

        if (Input.GetKeyDown(KeyCode.F) && Time.time >= nextUseTime)
        {
            nextUseTime = Time.time + cooldown;
            ResolveRoll(player);
        }
    }

    private void ResolveRoll(PlayerController player)
    {
        var pick = PickWeighted(outcomes, out Sprite icon);

        // Ícono sobre la cabeza
        if (icon != null)
            PopupIcon.Show(icon, player.transform.position + Vector3.up * 1.5f, 1.2f);

        // Efecto
        switch (pick)
        {
            case SlotOutcomeType.Nothing:
                break;

            case SlotOutcomeType.DamagePlayer_1:
            case SlotOutcomeType.DamagePlayer_1_Bis:
                GamblerEffects.DamagePlayer(player, 1);
                break;

            case SlotOutcomeType.DamageEnemies_10:
                GamblerEffects.DamageAllEnemies(10);
                break;

            case SlotOutcomeType.FreezeEnemies_2s:
                GamblerEffects.FreezeAllEnemies(2f);
                break;

            case SlotOutcomeType.SpeedEnemies_Plus5_3s:
                GamblerEffects.BoostEnemiesSpeed(+5f, 3f);
                break;

            case SlotOutcomeType.SpeedPlayer_Plus3_5s:
                GamblerEffects.BoostPlayerSpeed(player, +3f, 5f);
                break;

            case SlotOutcomeType.PlayerDamage_Plus1_5s:
                GamblerEffects.BoostPlayerDamage(player, +1, 5f);
                break;

            case SlotOutcomeType.Jackpot_1000_AllEnemies:
                GamblerEffects.DamageAllEnemies(1000);
                break;
        }
    }

    private SlotOutcomeType PickWeighted(List<WeightedOutcome> list, out Sprite icon)
    {
        icon = null;
        if (list == null || list.Count == 0) return SlotOutcomeType.Nothing;

        float total = 0f;
        foreach (var w in list) total += Mathf.Max(0f, w.weight);
        if (total <= 0f) return SlotOutcomeType.Nothing;

        float r = Random.value * total;
        float acc = 0f;
        foreach (var w in list)
        {
            float ww = Mathf.Max(0f, w.weight);
            acc += ww;
            if (r <= acc)
            {
                icon = w.iconOverHead;
                return w.type;
            }
        }
        return list[list.Count - 1].type; // fallback
    }
}
