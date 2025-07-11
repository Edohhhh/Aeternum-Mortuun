using UnityEngine;
using System.Collections.Generic;
using TMPro;
using EasyUI.PickerWheelUI;

public class WheelSelector : MonoBehaviour
{
    [Header("Prefabs de ruletas disponibles")]
    [SerializeField] private List<GameObject> ruletaPool; // mínimo 3

    [Header("Contenedor con HorizontalLayoutGroup")]
    [SerializeField] private Transform ruletaContainer;

    [Header("Texto opcional para debug de selección")]
    [SerializeField] private TextMeshProUGUI selectedLabel;

    [Header("UI sets (uno por ruleta)")]
    [SerializeField] private List<RuletaUISet> ruletaUISets;

    private List<PickerWheel> ruletasInstanciadas = new List<PickerWheel>();
    private PickerWheel ruletaSeleccionada;

    private void Start()
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

        // Selección aleatoria de prefabs distintos
        List<GameObject> seleccionadas = new List<GameObject>();
        List<GameObject> copiaPool = new List<GameObject>(ruletaPool);

        for (int i = 0; i < 3 && copiaPool.Count > 0; i++)
        {
            int idx = Random.Range(0, copiaPool.Count);
            seleccionadas.Add(copiaPool[idx]);
            copiaPool.RemoveAt(idx);
        }

        for (int i = 0; i < seleccionadas.Count; i++)
        {
            GameObject obj = Instantiate(seleccionadas[i], ruletaContainer);
            obj.name = $"Ruleta {i + 1}";

            PickerWheel wheel = obj.GetComponent<PickerWheel>();
            ruletasInstanciadas.Add(wheel);

            // ✅ FORZAR generación de contenido para que los íconos y efectos coincidan
            wheel.CargarPremiosDesdePool();

            // Vincular UISet con ruleta
            ruletaUISets[i].Inicializar(wheel, this);
        }
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
            wheel.Spin();

            // Desactivar UI cuando se agoten los usos
            if (wheel.UsosRestantes == 1)
            {
                wheel.AddSpinEndListener((_) =>
                {
                    if (wheel.UsosRestantes <= 0)
                    {
                        foreach (var set in ruletaUISets)
                        {
                            if (set.linkedWheel == wheel)
                            {
                                set.Activar(false);
                                Debug.Log($"❌ Botones de {wheel.name} desactivados (usos agotados)");
                            }
                        }
                    }
                });
            }
        }
    }

    public void ConfirmarRuleta(PickerWheel wheel)
    {
        if (wheel != null)
        {
            WheelPiece premio = wheel.ObtenerUltimoPremio();
            if (premio != null)
            {
                Debug.Log($"✅ Premio confirmado: {premio.Label} x{premio.Amount}");

                wheel.AplicarUltimoPremio();

                foreach (var set in ruletaUISets)
                {
                    if (set.linkedWheel == wheel)
                    {
                        set.ActualizarTextoSpin();

                        if (wheel.UsosRestantes <= 0)
                        {
                            set.Activar(false);
                            Debug.Log($"🔒 Botones desactivados para {wheel.name}");
                        }
                    }
                }
            }
        }
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