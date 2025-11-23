using UnityEngine;
using System.Collections.Generic;
using TMPro;
using EasyUI.PickerWheelUI;
using System.Collections;

[System.Serializable]
public class WeightedRuleta
{
    public GameObject prefab;
    [Range(0f, 100f)] public float weight = 1f;
}

public class WheelSelector : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private List<WeightedRuleta> ruletaWeightedPool;
    [SerializeField] private Transform ruletaContainer;
    [SerializeField] private List<RuletaUISet> ruletaUISets;
    [SerializeField] private GameObject confettiPrefab;

    [Header("Referencias UI")]
    [SerializeField] private WheelUIController wheelUIController;
    [SerializeField] private RewardPopupUI rewardPopup;

    private List<PickerWheel> ruletasInstanciadas = new List<PickerWheel>();
    private PickerWheel ruletaSeleccionada;
    private RuletaUISet uiSetSeleccionado;
    private WheelPiece premioPendiente;

    // Variables de Spins
    private int spinsBase = 3;
    private int spinsTotales;
    private int spinsRestantes;

    public void IniciarSelector()
    {
        CalcularSpins();
        InstanciarRuletasAleatorias();
    }

    // ✅ Lógica del Perk ExtraSpin
    private void CalcularSpins()
    {
        int extra = 0;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null) extra = pc.extraSpins;
        }
        spinsTotales = spinsBase + extra;
        spinsRestantes = spinsTotales;
    }

    public void InstanciarRuletasAleatorias()
    {
        foreach (Transform child in ruletaContainer) Destroy(child.gameObject);
        ruletasInstanciadas.Clear();

        List<WeightedRuleta> poolCopia = new List<WeightedRuleta>(ruletaWeightedPool);

        for (int i = 0; i < 3; i++)
        {
            if (poolCopia.Count == 0) break;

            WeightedRuleta seleccion = GetRandomWeightedRuleta(poolCopia);
            if (seleccion != null)
            {
                GameObject go = Instantiate(seleccion.prefab, ruletaContainer);
                PickerWheel pw = go.GetComponent<PickerWheel>();
                if (pw != null)
                {
                    ruletasInstanciadas.Add(pw);
                    if (i < ruletaUISets.Count)
                        ruletaUISets[i].Inicializar(pw, this);
                }
                poolCopia.Remove(seleccion);
            }
        }
    }

    private WeightedRuleta GetRandomWeightedRuleta(List<WeightedRuleta> pool)
    {
        float totalWeight = 0f;
        foreach (var item in pool) totalWeight += item.weight;
        float r = Random.Range(0f, totalWeight);
        float c = 0f;
        foreach (var item in pool) { c += item.weight; if (r <= c) return item; }
        return pool.Count > 0 ? pool[0] : null;
    }

    // 1. SELECCIONAR
    public void SeleccionarRuletaDesdeBoton(RuletaUISet uiSet)
    {
        uiSetSeleccionado = uiSet;
        ruletaSeleccionada = uiSet.linkedWheel;

        foreach (var set in ruletaUISets)
        {
            if (set == uiSetSeleccionado)
            {
                set.ModoGiro();
                // ✅ Actualizar Texto: "GIRAR (3/3)"
                set.ActualizarTextoSpin(spinsRestantes, spinsTotales);
            }
            else
            {
                set.Desactivar(true); // Oscurecer las otras
            }
        }

        if (wheelUIController != null) wheelUIController.ActualizarInstruccion("¡Haz girar la ruleta!");
    }

    // 2. GIRAR
    public void SpinRuleta(PickerWheel wheel)
    {
        if (wheel == null) return;
        if (wheel.IsSpinning) return;

        // No permitir girar si ya no quedan spins
        if (spinsRestantes <= 0) return;

        // Restamos visualmente
        spinsRestantes--;

        if (uiSetSeleccionado != null)
        {
            uiSetSeleccionado.ModoGirando();
            uiSetSeleccionado.ActualizarTextoSpin(spinsRestantes, spinsTotales);
        }

        wheel.onSpinEndEvent = (piece) =>
        {
            premioPendiente = piece;

            // ✅ Si aún quedan spins, permitir volver a girar o confirmar
            if (spinsRestantes > 0)
            {
                if (uiSetSeleccionado != null)
                    uiSetSeleccionado.ModoSpinYConfirmar();

                if (wheelUIController != null)
                    wheelUIController.ActualizarInstruccion("Puedes volver a girar o confirmar la recompensa.");
            }
            else
            {
                // ✅ Sin spins restantes: solo confirmar
                if (uiSetSeleccionado != null)
                    uiSetSeleccionado.ModoConfirmar();

                if (wheelUIController != null)
                    wheelUIController.ActualizarInstruccion("¡Confirma tu recompensa!");
            }
        };

        wheel.Spin();
    }

    // 3. CONFIRMAR (Cierre Instantáneo)
    public void ConfirmarPremio()
    {
        if (premioPendiente == null) return;

        // Aplicar Efecto
        if (premioPendiente.Effect != null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                premioPendiente.Effect.Apply(player);
                var pc = player.GetComponent<PlayerController>();
                if (pc != null) pc.SavePlayerData();
            }
        }

        // Popup (Quedará en pantalla mientras juegas)
        if (rewardPopup != null)
        {
            string name = (premioPendiente.Effect != null) ? premioPendiente.Effect.label : premioPendiente.Label;
            string desc = (premioPendiente.Effect != null) ? premioPendiente.Effect.description : "";
            rewardPopup.ShowReward(premioPendiente.Icon, name, desc);
        }

        // Confetti
        if (confettiPrefab != null)
        {
            confettiPrefab.SetActive(true);
            StartCoroutine(DesactivarConfetti(confettiPrefab, 2f));
        }

        // ✅ CIERRE INSTANTÁNEO:
        // 1. Ocultamos la UI de ruleta YA.
        if (wheelUIController != null) wheelUIController.OcultarTodo();

        // 2. Reanudamos el juego YA.
        Time.timeScale = 1f;

        // 3. Si hay cambio de sala, lo activamos
        if (RoomManager.Instance != null) RoomManager.Instance.LoadNextRoomWithDelay();
    }

    private IEnumerator DesactivarConfetti(GameObject confetti, float delay)
    {
        yield return new WaitForSeconds(delay); // TimeScale ya es 1, usamos Seconds normal
        confetti.SetActive(false);
    }
}
