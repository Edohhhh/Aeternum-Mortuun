using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection; // Para leer stats privadas
using System.Collections.Generic;
using DG.Tweening;

[DefaultExecutionOrder(200)]
public class PlayerStatsHUD : MonoBehaviour
{
    [Header("Mostrar/Ocultar")]
    [SerializeField] private bool holdToShow = true;
    [SerializeField] private bool toggleMode = false;
    [SerializeField] private KeyCode key = KeyCode.Tab;

    [Header("Panel contenedor")]
    [SerializeField] private GameObject panel;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panelRectTransform;

    [Header("Animación")]
    [SerializeField] private float slideDuration = 0.3f;
    [SerializeField] private Vector2 slideOffset = new Vector2(0, -50);

    // ✅ --- Referencias para Perks (Tu nueva lógica de inventario) ---
    [Header("Perks (Inventario)")]
    [SerializeField] private Transform perksContainer; // El objeto con el Horizontal Layout Group
    [SerializeField] private GameObject perkIconPrefab; // El prefab del icono
    [SerializeField] private InventoryTooltipUI tooltipUI; // El tooltip de la escena
    // ✅ --- FIN ---

    [Header("Filas (Texto de Stats)")]
    // (Tu lista de stats de texto)
    [SerializeField] private TMP_Text speedTxt;
    [SerializeField] private TMP_Text dashSpeedTxt;
    [SerializeField] private TMP_Text dashIframesTxt;
    [SerializeField] private TMP_Text dashSlideDurTxt;
    [SerializeField] private TMP_Text dashDurTxt;
    [SerializeField] private TMP_Text dashCooldownTxt;

    [SerializeField] private TMP_Text maxHealthTxt;
    [SerializeField] private TMP_Text currentHealthTxt;
    [SerializeField] private TMP_Text regenRateTxt;
    [SerializeField] private TMP_Text regenDelayTxt;

    [SerializeField] private TMP_Text baseDamageTxt;
    [SerializeField] private TMP_Text attackCooldownTxt;
    [SerializeField] private TMP_Text attackCooldownRemainingTxt;
    [SerializeField] private TMP_Text recoilDistanceTxt; // Cambiado de knockbackForceTxt
    // ...y todas las demás que tengas...


    // --- Variables de Estado y Referencias ---
    private bool isPanelVisible = false;
    private bool isTransitioning = false;

    // ✅ --- Referencias Corregidas ---
    private PlayerController player;
    private PlayerHealth health;
    private CombatSystem combat; // Corregido de PlayerCombat

    // Reflection (para leer stats privadas)
    private FieldInfo attackCooldownTimerField;

    private void Start()
    {
        // Encontrar al jugador
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            // ✅ Obtenemos los componentes correctos
            player = playerObj.GetComponent<PlayerController>();
            health = playerObj.GetComponent<PlayerHealth>();
            combat = playerObj.GetComponent<CombatSystem>();
        }
        else
        {
            Debug.LogError("PlayerStatsHUD: No se encontró el GameObject 'Player'.");
        }

        // Reflection (para leer el timer privado 'attackTimer' de CombatSystem)
        if (combat != null)
        {
            // ✅ Corregido a 'attackTimer' (minúscula)
            attackCooldownTimerField = combat.GetType().GetField("attackTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            if (attackCooldownTimerField == null)
            {
                Debug.LogWarning("PlayerStatsHUD: No se pudo encontrar el Field 'attackTimer' en CombatSystem.cs");
            }
        }

        // Ocultar panel al inicio
        if (panel != null) panel.SetActive(false);
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }

    private void Update()
    {
        HandleInput();

        // Si el panel es visible, actualiza las stats
        if (isPanelVisible)
        {
            UpdateStatsText();
        }
    }

    private void HandleInput()
    {
        if (toggleMode)
        {
            if (Input.GetKeyDown(key))
            {
                TogglePanel();
            }
        }
        else if (holdToShow)
        {
            if (Input.GetKeyDown(key))
            {
                ShowPanel();
            }
            else if (Input.GetKeyUp(key))
            {
                HidePanel();
            }
        }
    }

    public void TogglePanel()
    {
        if (isPanelVisible)
            HidePanel();
        else
            ShowPanel();
    }

    public void ShowPanel()
    {
        if (isTransitioning || isPanelVisible) return;
        isTransitioning = true;
        isPanelVisible = true;

        if (panel != null) panel.SetActive(true);

        // --- Lógica del Inventario de Perks ---
        UpdatePerksUI();
        // --- Fin ---

        // Actualiza las stats de texto
        UpdateStatsText();

        // Animación de Fade In y Deslizamiento
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(1f, slideDuration)
                .From(0f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true); // Ignora la pausa
        }
        if (panelRectTransform != null)
        {
            panelRectTransform.DOAnchorPos(Vector2.zero, slideDuration)
                .From(slideOffset)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true) // Ignora la pausa
                .OnComplete(() => isTransitioning = false);
        }
    }

    public void HidePanel()
    {
        if (isTransitioning || !isPanelVisible) return;
        isTransitioning = true;
        isPanelVisible = false;

        // Oculta el tooltip por si acaso
        if (tooltipUI != null)
            tooltipUI.Hide();

        // Animación de Fade Out y Deslizamiento
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0f, slideDuration)
                .From(1f)
                .SetEase(Ease.InQuad)
                .SetUpdate(true); // Ignora la pausa
        }
        if (panelRectTransform != null)
        {
            panelRectTransform.DOAnchorPos(slideOffset, slideDuration)
                .From(Vector2.zero)
                .SetEase(Ease.InQuad)
                .SetUpdate(true) // Ignora la pausa
                .OnComplete(() =>
                {
                    isTransitioning = false;
                    if (panel != null) panel.SetActive(false);
                });
        }
    }

    // --- MÉTODO PARA ACTUALIZAR LOS ICONOS DE PERKS (Tu nueva lógica) ---
    private void UpdatePerksUI()
    {
        if (perksContainer == null || perkIconPrefab == null)
        {
            Debug.LogError("PlayerStatsHUD: Faltan referencias de Perks Container o Prefab del Icono.");
            return;
        }

        // 1. Limpiar iconos de perks anteriores
        foreach (Transform child in perksContainer)
        {
            Destroy(child.gameObject);
        }

        if (tooltipUI == null)
        {
            Debug.LogError("PlayerStatsHUD: Tooltip UI no está asignado.");
            return;
        }

        // 2. Cargar perks actuales del jugador (desde PlayerController.initialPowerUps)
        if (player != null && player.initialPowerUps != null)
        {
            // Iteramos sobre el array de PowerUp[]
            foreach (PowerUp powerUp in player.initialPowerUps)
            {
                // El PowerUp debe tener su 'PowerUpEffect' asignado
                if (powerUp != null && powerUp.effect != null)
                {
                    // 3. Instanciar un icono para esta perk
                    GameObject iconObj = Instantiate(perkIconPrefab, perksContainer);
                    PerkIconUI iconScript = iconObj.GetComponent<PerkIconUI>();

                    if (iconScript != null)
                    {
                        // 4. Inicializar el icono con los datos del SO y la ref al tooltip
                        iconScript.Initialize(powerUp.effect, tooltipUI);
                    }
                }
            }
        }
    }


    // ✅ --- MÉTODO DE STATS CORREGIDO ---
    private void UpdateStatsText()
    {
        // --- Stats de PlayerHealth ---
        if (health != null)
        {
            SetText(maxHealthTxt, health.maxHealth);
            SetText(currentHealthTxt, health.currentHealth);
            SetText(regenRateTxt, health.regenerationRate);
            SetText(regenDelayTxt, health.regenDelay);
        }

        // --- Stats de PlayerController (Movimiento y Daño) ---
        if (player != null)
        {
            // Movimiento
            SetText(speedTxt, player.moveSpeed);
            SetText(dashSpeedTxt, player.dashSpeed);
            SetText(dashIframesTxt, player.dashIframes);
            SetText(dashSlideDurTxt, player.dashSlideDuration);
            SetText(dashDurTxt, player.dashDuration);
            SetText(dashCooldownTxt, player.dashCooldown);

            // Combate
            SetText(baseDamageTxt, player.baseDamage);
        }

        // --- Stats de CombatSystem ---
        if (combat != null)
        {
            SetText(attackCooldownTxt, combat.attackCooldown);
            SetText(recoilDistanceTxt, combat.recoilDistance); // Mapeado aquí

            // Leer timer privado con Reflection
            if (attackCooldownRemainingTxt != null && attackCooldownTimerField != null)
            {
                var val = attackCooldownTimerField.GetValue(combat);
                float remaining = (val is float f) ? Mathf.Max(0f, f) : 0f;
                attackCooldownRemainingTxt.text = $"{remaining:0.00}";
            }
        }
    }

    // Helper para asignar texto (simplificado)
    private static void SetText(TMP_Text t, float value)
    {
        if (t == null) return;
        t.text = $"{value:0.##}";
    }
}