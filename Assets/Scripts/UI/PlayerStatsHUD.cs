using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;
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
    [SerializeField] private RectTransform panelRectTransform; // Nuevo: para la animación

    [Header("Animación")]
    [SerializeField] private float slideDuration = 0.3f;
    [SerializeField] private Vector2 slideOffset = new Vector2(0, -50); // Desplazamiento inicial

    [Header("Filas (Texto)")]
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
    [SerializeField] private TMP_Text invulnTimeTxt;

    [SerializeField] private TMP_Text posTxt;
    [SerializeField] private TMP_Text powerupsTxt;

    // --- Ataque (de CombatSystem) ---
    [Header("Ataque (Cooldown)")]
    [SerializeField] private TMP_Text attackCooldownTxt;
    [SerializeField] private TMP_Text attackCooldownRemainingTxt;

    // --- NUEVO: stats de daño/knockback del PlayerController ---
    [Header("Daño / Knockback (PlayerController)")]
    [SerializeField] private TMP_Text baseDamageTxt;
    [SerializeField] private TMP_Text knockbackForceTxt;

    [Header("Perks (Imágenes)")]
    public List<string> perkAssetNames = new List<string>();   // Nombres de los assets PowerUpEffect
    public List<Sprite> perkIcons = new List<Sprite>();       // Imágenes correspondientes
    public List<Transform> perkSpawnPoints = new List<Transform>(); // Lugares donde aparecerán

    [SerializeField, Min(0.05f)] private float refreshInterval = 0.25f;
    private float refreshTimer;

    private bool _visible;
    private Vector2 originalAnchoredPosition; // Guardar la posición original
    private GameDataManager DM => GameDataManager.Instance;

    // refs cache
    private PlayerController player;
    private CombatSystem combat;

    // reflection cache
    private FieldInfo attackTimerField;
    private FieldInfo knockbackForceField;

    // Cache de iconos instanciados
    private List<GameObject> instantiatedIcons = new List<GameObject>();

    private void Awake()
    {
        if (panel == null) panel = gameObject;
        if (panelRectTransform == null) panelRectTransform = GetComponent<RectTransform>();

        // Guardar la posición original
        originalAnchoredPosition = panelRectTransform.anchoredPosition;

        ApplyVisible(false, true);

        player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            combat = player.GetComponent<CombatSystem>();
            if (combat != null)
            {
                attackTimerField = typeof(CombatSystem)
                    .GetField("attackTimer", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            knockbackForceField = typeof(PlayerController)
                .GetField("knockbackForce", BindingFlags.Instance | BindingFlags.NonPublic);
        }
    }

    private void Update()
    {
        HandleInput();

        if (_visible)
        {
            refreshTimer -= Time.unscaledDeltaTime;
            if (refreshTimer <= 0f)
            {
                refreshTimer = refreshInterval;
                RefreshHUD();
            }
        }
    }

    private void HandleInput()
    {
        if (holdToShow && !toggleMode)
        {
            bool shouldShow = Input.GetKey(key);
            if (shouldShow != _visible) ApplyVisible(shouldShow);
            return;
        }

        if (Input.GetKeyDown(key))
        {
            _visible = !_visible;
            ApplyVisible(_visible);
        }
    }

    private void ApplyVisible(bool show, bool force = false)
    {
        if (!force && _visible == show) return;
        _visible = show;
        refreshTimer = 0f;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = show ? 1f : 0f;
            canvasGroup.blocksRaycasts = show;
            canvasGroup.interactable = show;
        }

        // Animar el panel
        AnimatePanel(show);

        // Mostrar/Ocultar iconos de perks
        if (show)
        {
            ShowPerkIcons();
        }
        else
        {
            HidePerkIcons();
        }
    }

    private void AnimatePanel(bool show)
    {
        Vector2 targetPosition = show ? originalAnchoredPosition : originalAnchoredPosition + slideOffset;

        panelRectTransform.anchoredPosition = show ? originalAnchoredPosition + slideOffset : originalAnchoredPosition;

        panelRectTransform
            .DOAnchorPos(targetPosition, slideDuration)
            .SetEase(Ease.OutBack);
    }

    private void ShowPerkIcons()
    {
        HidePerkIcons(); // Limpiar iconos anteriores

        var perks = DM.playerData.initialPowerUps;
        var tooltip = FindObjectOfType<InventoryTooltipUI>(true); // 🧠 Buscar el tooltip en la escena

        foreach (var powerUp in perks)
        {
            if (powerUp != null && powerUp.effect != null)
            {
                string assetName = powerUp.effect.name;

                int index = perkAssetNames.IndexOf(assetName);
                if (index != -1 && index < perkIcons.Count && index < perkSpawnPoints.Count)
                {
                    var spawnPoint = perkSpawnPoints[index];
                    var iconGO = new GameObject("PerkIcon");
                    iconGO.transform.SetParent(spawnPoint.parent);
                    iconGO.transform.position = spawnPoint.position;

                    var image = iconGO.AddComponent<Image>();
                    image.sprite = perkIcons[index];
                    instantiatedIcons.Add(iconGO);

                    // 🧩 Agregar EventTrigger para tooltip
                    var trigger = iconGO.AddComponent<UnityEngine.EventSystems.EventTrigger>();

                    // Mouse entra → mostrar tooltip
                    var entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry
                    {
                        eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
                    };
                    entryEnter.callback.AddListener((_) =>
                    {
                        if (tooltip != null && powerUp.effect != null)
                        {
                            tooltip.Show(powerUp.effect.label, powerUp.effect.description);
                        }
                    });
                    trigger.triggers.Add(entryEnter);

                    // Mouse sale → ocultar tooltip
                    var entryExit = new UnityEngine.EventSystems.EventTrigger.Entry
                    {
                        eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit
                    };
                    entryExit.callback.AddListener((_) =>
                    {
                        if (tooltip != null)
                            tooltip.Hide();
                    });
                    trigger.triggers.Add(entryExit);
                }
            }
        }
    }

    private void HidePerkIcons()
    {
        foreach (var icon in instantiatedIcons)
        {
            if (icon != null) Destroy(icon);
        }
        instantiatedIcons.Clear();
    }

    private void RefreshHUD()
    {
        if (DM == null || DM.playerData == null) return;
        var d = DM.playerData;

        // Movimiento / Dash
        SetText(speedTxt, d.moveSpeed, "Velocidad");
        SetText(dashSpeedTxt, d.dashSpeed, "Dash Vel.");
        SetText(dashIframesTxt, d.dashIframes, "Dash iFrames");
        SetText(dashSlideDurTxt, d.dashSlideDuration, "Dash Slide");
        SetText(dashDurTxt, d.dashDuration, "Dash Duración");
        SetText(dashCooldownTxt, d.dashCooldown, "Dash CD");

        // Salud
        SetText(maxHealthTxt, d.maxHealth, "Vida Máx");
        SetText(currentHealthTxt, d.currentHealth, "Vida");
        SetText(regenRateTxt, d.regenerationRate, "Regen/s");
        SetText(regenDelayTxt, d.regenDelay, "Delay Regen");
        SetText(invulnTimeTxt, d.invulnerableTime, "Invulnerable");

        // Pos y PowerUps
        if (posTxt != null)
            posTxt.text = $"Pos: X {d.position.x:0.##}  Y {d.position.y:0.##}";
        if (powerupsTxt != null)
            powerupsTxt.text = $"PowerUps: {d.initialPowerUps?.Count ?? 0}";

        // --- Ataque (cooldown) desde CombatSystem ---
        if (combat != null)
        {
            if (attackCooldownTxt != null)
                attackCooldownTxt.text = $"{combat.attackCooldown:0.00}";
            if (attackCooldownRemainingTxt != null && attackTimerField != null)
            {
                var val = attackTimerField.GetValue(combat);
                float remaining = (val is float f) ? Mathf.Max(0f, f) : 0f;
                attackCooldownRemainingTxt.text = $"{remaining:0.00}";
            }
        }
        else
        {
            if (attackCooldownTxt != null) attackCooldownTxt.text = "-";
            if (attackCooldownRemainingTxt != null) attackCooldownRemainingTxt.text = "-";
        }

        // --- NUEVO: Base Damage y Knockback Force desde PlayerController ---
        if (player != null)
        {
            if (baseDamageTxt != null)
                baseDamageTxt.text = $"{player.baseDamage}";

            if (knockbackForceTxt != null)
            {
                float kf = 0f;
                if (knockbackForceField != null)
                {
                    var val = knockbackForceField.GetValue(player);
                    if (val is float f) kf = f;
                }
                knockbackForceTxt.text = $"{kf:0.##}";
            }
        }
        else
        {
            if (baseDamageTxt != null) baseDamageTxt.text = "-";
            if (knockbackForceTxt != null) knockbackForceTxt.text = "-";
        }
    }

    private static void SetText(TMP_Text t, float value, string label)
    {
        if (t == null) return;
        t.text = $"{value:0.##}";
    }

    private static void SetText(TMP_Text t, int value, string label)
    {
        if (t == null) return;
        t.text = $"{value}";
    }
}