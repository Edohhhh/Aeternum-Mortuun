using UnityEngine;
using System.Collections.Generic;
using TMPro;
using EasyUI.PickerWheelUI;
using System.Collections;

[System.Serializable]
public class WeightedRuleta
{
    public GameObject prefab;
    [Range(0f, 100f)] public float weight = 1f; // porcentaje/ponderación de aparición
}

public class WheelSelector : MonoBehaviour
{
    [Header("Ruletas ponderadas (elige 3 sin reemplazo por porcentaje)")]
    [SerializeField] private List<WeightedRuleta> ruletaWeightedPool; // mínima 3 válidas (peso > 0 y prefab asignado)

    [Header("Contenedor con HorizontalLayoutGroup")]
    [SerializeField] private Transform ruletaContainer;

    [Header("Texto opcional para debug de selección")]
    [SerializeField] private TextMeshProUGUI selectedLabel;

    [Header("UI sets (uno por ruleta)")]
    [SerializeField] private List<RuletaUISet> ruletaUISets;

    [Header("Efectos de celebración")]
    [SerializeField] private GameObject confettiPrefab;

    [Header("Controlador de la UI de ruletas")]
    [SerializeField] private EasyUI.PickerWheelUI.WheelUIController wheelUIController;



    private List<PickerWheel> ruletasInstanciadas = new List<PickerWheel>();
    private PickerWheel ruletaSeleccionada;

    public void IniciarSelector()
    {
        InstanciarRuletasAleatorias();
    }

    public void InstanciarRuletasAleatorias()
    {
        // Limpiar ruletas anteriores
        foreach (Transform child in ruletaContainer)
            Destroy(child.gameObject);

        ruletasInstanciadas.Clear();
        ruletaSeleccionada = null;

        // Validar y clonar lista filtrada (prefab != null, weight > 0)
        List<WeightedRuleta> candidatos = new List<WeightedRuleta>();
        foreach (var w in ruletaWeightedPool)
        {
            if (w != null && w.prefab != null && w.weight > 0f)
                candidatos.Add(w);
        }

        if (candidatos.Count < 3)
        {
            Debug.LogError("❌ Necesitas al menos 3 ruletas válidas (prefab asignado y weight > 0) en 'ruletaWeightedPool'.");
            return;
        }

        PlayerController player = null;
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.GetComponent<PlayerController>();

        if (player == null)
            Debug.LogError("❌ WheelSelector no pudo encontrar al PlayerController.");

        // Elegir 3 distintas por ponderación (sin reemplazo)
        List<WeightedRuleta> seleccionadas = PickDistinctWeighted(candidatos, 3);

        // Instanciar y configurar
        for (int i = 0; i < seleccionadas.Count; i++)
        {
            GameObject obj = Instantiate(seleccionadas[i].prefab, ruletaContainer);
            obj.name = $"Ruleta {i + 1}";

            PickerWheel wheel = obj.GetComponent<PickerWheel>();
            if (wheel == null)
            {
                Debug.LogError($"❌ El prefab '{seleccionadas[i].prefab.name}' no tiene PickerWheel.");
                continue;
            }

            ruletasInstanciadas.Add(wheel);

            // Sincroniza los spins ANTES de inicializar la UI
            if (player != null)
            {
                // Aquí se aplica el Extra Spin
                wheel.SincronizarSpinsConPlayer(player);
            }

            // Carga los premios
            wheel.CargarPremiosDesdePoolsPonderados();

            // Vincular UISet si existe
            if (i < ruletaUISets.Count && ruletaUISets[i] != null)
            {
                ruletaUISets[i].Inicializar(wheel, this);
            }
            else
            {
                Debug.LogWarning($"⚠️ No hay UI set para la ruleta {i + 1}. Revisa la lista 'ruletaUISets'.");
            }
        }
    }

    // --- Selección ponderada sin reemplazo ---
    private List<WeightedRuleta> PickDistinctWeighted(List<WeightedRuleta> source, int k)
    {
        List<WeightedRuleta> pool = new List<WeightedRuleta>(source);
        List<WeightedRuleta> result = new List<WeightedRuleta>(k);

        for (int picks = 0; picks < k; picks++)
        {
            float total = 0f;
            foreach (var w in pool) total += Mathf.Max(0f, w.weight);

            if (total <= 0f)
            {
                result.Add(pool[0]);
                pool.RemoveAt(0);
                continue;
            }

            float r = Random.Range(0f, total);
            float acc = 0f;
            int chosenIndex = -1;

            for (int i = 0; i < pool.Count; i++)
            {
                acc += Mathf.Max(0f, pool[i].weight);
                if (r <= acc)
                {
                    chosenIndex = i;
                    break;
                }
            }

            if (chosenIndex < 0) chosenIndex = pool.Count - 1;

            result.Add(pool[chosenIndex]);
            pool.RemoveAt(chosenIndex);
        }

        return result;
    }

    public void SeleccionarRuletaDesdeBoton(RuletaUISet seleccionado)
    {
        ruletaSeleccionada = seleccionado.linkedWheel;

        foreach (var set in ruletaUISets)
        {
            set.selectButton.interactable = false;
            set.Activar(set == seleccionado);
        }

        Debug.Log($"🎯 Ruleta seleccionada: {ruletaSeleccionada.name}");
    }

    public void SeleccionarRuleta(PickerWheel seleccionada)
    {
        ruletaSeleccionada = seleccionada;

        if (selectedLabel != null)
            selectedLabel.text = $"Seleccionada: {seleccionada.name}";

        Debug.Log($"🎯 Ruleta seleccionada: {seleccionada.name}");
    }

    public void SpinRuleta(PickerWheel wheel)
    {
        if (wheel != null && !wheel.IsSpinning && wheel.UsosRestantes > 0)
        {
            // Desactivar Confirm mientras gira
            foreach (var set in ruletaUISets)
            {
                if (set.linkedWheel == wheel && set.confirmButton != null)
                    set.confirmButton.interactable = false;
            }

            // Iniciar el giro
            wheel.Spin();

            // Cuando termina de girar, volver a habilitar Confirm
            wheel.AddSpinEndListener((_) =>
            {
                foreach (var set in ruletaUISets)
                {
                    if (set.linkedWheel == wheel)
                    {
                        set.ActualizarTextoSpin();

                        if (set.confirmButton != null)
                        {
                            set.confirmButton.interactable = true;
                        }
                    }
                }
            });

            // Si solo queda 1 spin, preparar el bloqueo de spin
            if (wheel.UsosRestantes == 1)
            {
                wheel.AddSpinEndListener((_) =>
                {
                    foreach (var set in ruletaUISets)
                    {
                        if (set.linkedWheel == wheel)
                            set.ActualizarTextoSpin();
                    }

                    if (wheel.UsosRestantes <= 0)
                    {
                        foreach (var set in ruletaUISets)
                        {
                            if (set.linkedWheel == wheel)
                            {
                                if (set.spinButton != null)
                                    set.spinButton.interactable = false;
                                if (set.confirmButton != null)
                                    set.confirmButton.interactable = true;
                            }
                        }
                    }
                });
            }
        }
    }


    // ✅ --- MÉTODO MODIFICADO ---
    public void ConfirmarRuleta(PickerWheel wheel)
    {
        // ✅ --- AÑADIDO: Limpiar "AcidPoollChico(Clone)" ---
        // Esto se ejecuta ANTES que el resto de la lógica de confirmación.

        string targetName = "AcidPoollChico(Clone)";
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int count = 0;

        foreach (GameObject obj in allObjects)
        {
            // Comprueba si el nombre es exactamente el solicitado
            if (obj.name == targetName)
            {
                GameObject.Destroy(obj);
                count++;
            }
        }

        if (count > 0)
        {
            Debug.Log($"🧹 Se eliminaron {count} objetos '{targetName}'.");
        }
        // ✅ --- FIN DEL CÓDIGO AÑADIDO ---


        // --- Lógica de confirmación original ---
        if (wheel == null)
        {
            Debug.LogWarning("⚠️ No se asignó ruleta.");
            return;
        }

        // APLICA el power-up (efecto real del premio)
        wheel.AplicarUltimoPremio();

        // Muestra el popup del premio
        wheel.MostrarPopupUltimoPremio();

        // Efecto visual de confetti
        if (confettiPrefab != null)
        {
            confettiPrefab.SetActive(true);
            ParticleSystem ps = confettiPrefab.GetComponent<ParticleSystem>();
            float duracion = 2f;
            if (ps != null)
                duracion = ps.main.duration + ps.main.startLifetime.constantMax;
            StartCoroutine(DesactivarConfetti(confettiPrefab, duracion));
        }

        // Guardar datos del jugador
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                GameDataManager.Instance.SavePlayerData(playerController);
                Debug.Log("📦 Datos del jugador guardados tras confirmar ruleta.");
            }
            else
            {
                Debug.LogError("❌ No se encontró PlayerController en el objeto del jugador.");
            }
        }
        else
        {
            Debug.LogError("❌ No se encontró GameObject con tag 'Player'.");
        }

        // Cargar siguiente sala (descomentar si es necesario)
        //RoomManager.Instance.LoadNextRoomWithDelay();

        // Desactivar UI de todas las ruletas
        foreach (var set in ruletaUISets)
        {
            if (set != null)
                set.Activar(false);
        }

        // Avisar al controlador de UI de ruletas que cierre HUD y reanude el juego
        if (wheelUIController != null)
        {
            wheelUIController.ConfirmarPremio();
        }
        else
        {
            Debug.LogWarning("⚠️ No se asignó WheelUIController en WheelSelector.");
        }
    }


    // --- Coroutine para desactivar el confetti ---
    private IEnumerator DesactivarConfetti(GameObject confetti, float delay)
    {
        yield return new WaitForSeconds(delay);
        confetti.SetActive(false);
    }

    public void SpinRuletaSeleccionada()
    {
        if (ruletaSeleccionada != null && !ruletaSeleccionada.IsSpinning)
        {
            ruletaSeleccionada.Spin();
        }
    }

    public void ConfirmarRuletaSeleccionada()
    {
        if (ruletaSeleccionada != null)
        {
            WheelPiece premio = ruletaSeleccionada.ObtenerUltimoPremio();
            if (premio != null)
                Debug.Log($"✅ Premio confirmado: {premio.Label} x{premio.Amount}");
        }
    }

    public void MostrarNombreRuletaSeleccionada()
    {
        if (ruletaSeleccionada != null)
            Debug.Log($"🧩 Ruleta seleccionada es: {ruletaSeleccionada.name}");
    }
}