using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DarkSacrificeZone : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Factor de velocidad mientras está en la marca (ej: 0.25 = 75% menos)")]
    public float speedMultiplier = 0.25f;

    [Tooltip("Daño que gana el jugador cada vez")]
    public int damageGain = 1;

    [Tooltip("Cada cuántos segundos se gana daño")]
    public float intervalSeconds = 2f;

    // ---- Registro global por jugador ----
    private class PlayerState
    {
        public int insideCount;
        public float originalSpeed;
        public int originalDamage;
    }

    private static readonly Dictionary<PlayerController, PlayerState> states = new Dictionary<PlayerController, PlayerState>();

    private Coroutine damageRoutine;
    private PlayerController currentPlayer;

    private bool ApplyEffects(PlayerController pc)
    {
        if (!states.TryGetValue(pc, out var st))
        {
            st = new PlayerState
            {
                insideCount = 0,
                originalSpeed = pc.moveSpeed,
                originalDamage = pc.baseDamage
            };
            states[pc] = st;
        }

        st.insideCount++;
        if (st.insideCount == 1)
        {
            // Primera vez -> aplicar penalización de velocidad
            pc.moveSpeed = st.originalSpeed * speedMultiplier;
            return true;
        }
        return false;
    }

    private void RemoveEffects(PlayerController pc)
    {
        if (!states.TryGetValue(pc, out var st)) return;

        st.insideCount--;
        if (st.insideCount <= 0)
        {
            // Restaurar velocidad y daño original
            pc.moveSpeed = st.originalSpeed;
            pc.baseDamage = st.originalDamage;
            states.Remove(pc);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var controller = other.GetComponentInParent<PlayerController>();
        if (controller == null) return;

        currentPlayer = controller;
        bool firstApply = ApplyEffects(controller);

        if (firstApply && damageRoutine == null)
            damageRoutine = StartCoroutine(GainDamageOverTime(controller));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var controller = other.GetComponentInParent<PlayerController>();
        if (controller == null) return;

        RemoveEffects(controller);

        if (!states.ContainsKey(controller))
        {
            if (damageRoutine != null)
            {
                StopCoroutine(damageRoutine);
                damageRoutine = null;
            }
            if (controller == currentPlayer) currentPlayer = null;
        }
    }

    private void OnDisable()
    {
        if (currentPlayer != null)
        {
            RemoveEffects(currentPlayer);
            currentPlayer = null;
        }
        if (damageRoutine != null)
        {
            StopCoroutine(damageRoutine);
            damageRoutine = null;
        }
    }

    private void OnDestroy()
    {
        if (currentPlayer != null)
        {
            RemoveEffects(currentPlayer);
            currentPlayer = null;
        }
        if (damageRoutine != null)
        {
            StopCoroutine(damageRoutine);
            damageRoutine = null;
        }
    }

    private IEnumerator GainDamageOverTime(PlayerController controller)
    {
        while (controller != null && states.ContainsKey(controller))
        {
            controller.baseDamage += damageGain;
            yield return new WaitForSeconds(intervalSeconds);
        }
        damageRoutine = null;
    }
}
