using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class LifeShrine : MonoBehaviour
{
    [Header("Diálogo (estilo NPC)")]
    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text nameText;
    public Image portraitImage;
    public float fallbackTypingSpeed = 0.02f;

    [Header("Interacción")]
    public KeyCode interactKey = KeyCode.F;
    public string playerTag = "Player";
    public Sprite interactIcon;
    public float iconOffsetY = 1.5f;

    [Header("Costo de entrada (vida PERMANENTE)")]
    [Tooltip("Cuántos corazones de VIDA MÁXIMA se pierden por apuesta (ej: 0.5 = medio corazón).")]
    [Min(0.1f)] public float vidaACobrar = 0.5f;

    [Tooltip("Límite inferior de vida máxima. Debajo de esto ya no deja apostar.")]
    public float minMaxHealth = 0.5f;

    [TextArea]
    public string noHealthMessage = "No tienes suficiente vida máxima para seguir apostando.";

    [Header("Ruleta")]
    public EasyUI.PickerWheelUI.WheelUIController wheelUIController;

    // ---- Estado interno ----
    private bool playerInRange;
    private bool isDialogueActive;
    private bool isTyping;
    private int dialogueIndex;
    private bool showedInsufficientLine;
    private SpriteRenderer runtimeIconRenderer;

    // Candado global para que no hablen 2 NPC/altares al mismo tiempo
    private static bool anyDialogueBusy = false;

    // Bloqueo por falta de vida hasta salir del trigger
    private bool blockedForNoHealthUntilExit = false;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    private void Awake()
    {
        if (interactIcon != null)
        {
            var go = new GameObject("InteractIcon");
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0f, iconOffsetY, 0f);
            runtimeIconRenderer = go.AddComponent<SpriteRenderer>();
            runtimeIconRenderer.sprite = interactIcon;
            runtimeIconRenderer.sortingOrder = 50;
            runtimeIconRenderer.enabled = false;
        }

        if (dialoguePanel) dialoguePanel.SetActive(false);
    }

    private void Start()
    {
        if (wheelUIController == null)
            wheelUIController = FindFirstObjectByType<EasyUI.PickerWheelUI.WheelUIController>();
    }

    private void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(interactKey))
        {
            // Iniciar diálogo
            if (!isDialogueActive &&
                !anyDialogueBusy &&
                !blockedForNoHealthUntilExit)
            {
                StartDialogue();
                return;
            }

            // Completar tipeo de la línea actual
            if (isDialogueActive && isTyping)
            {
                StopAllCoroutines();
                dialogueText.SetText(GetCurrentLineForDisplay());
                isTyping = false;
                return;
            }

            // En última línea
            if (isDialogueActive && !isTyping && IsLastLine())
            {
                if (HasEnoughHealth())
                {
                    ExecuteRouletteAndClose();
                }
                else
                {
                    // Primera vez: mostrar aviso
                    if (!showedInsufficientLine)
                    {
                        showedInsufficientLine = true;
                        StartTypingNewLine(noHealthMessage, GetTypingSpeed());
                    }
                    else
                    {
                        // Segunda vez: cerrar y bloquear hasta salir
                        blockedForNoHealthUntilExit = true;
                        CloseAndReset();
                    }
                }
                return;
            }

            // Avanzar diálogo normal
            if (isDialogueActive && !isTyping)
            {
                NextLine();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            if (!isDialogueActive &&
                runtimeIconRenderer &&
                !blockedForNoHealthUntilExit)
            {
                runtimeIconRenderer.enabled = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            blockedForNoHealthUntilExit = false;
            CloseAndReset();
        }
    }

    // ---------- Diálogo ----------
    private void StartDialogue()
    {
        if (dialogueData == null || dialoguePanel == null || dialogueText == null)
        {
            Debug.LogError("LifeShrine: Faltan referencias de diálogo en el Inspector.");
            return;
        }

        StopAllCoroutines();
        anyDialogueBusy = true;
        isDialogueActive = true;
        isTyping = false;
        dialogueIndex = 0;
        showedInsufficientLine = false;

        if (nameText) nameText.SetText(dialogueData.npcName);
        if (portraitImage) portraitImage.sprite = dialogueData.npcPortrait;

        dialogueText.text = "";
        dialoguePanel.SetActive(true);
        if (runtimeIconRenderer) runtimeIconRenderer.enabled = false;

        StartTypingNewLine(GetCurrentLine(), GetTypingSpeed());
    }

    private void NextLine()
    {
        dialogueIndex++;
        if (dialogueIndex < SafeLinesCount())
        {
            StartTypingNewLine(GetCurrentLine(), GetTypingSpeed());
        }
        else
        {
            CloseAndReset();
        }
    }

    private void StartTypingNewLine(string line, float speed)
    {
        StopAllCoroutines();
        dialogueText.text = "";
        StartCoroutine(TypeLine(line, speed));
    }

    private IEnumerator TypeLine(string line, float speed)
    {
        isTyping = true;
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(speed);
        }
        isTyping = false;
    }

    // ---------- Ejecución Ruleta ----------
    private void ExecuteRouletteAndClose()
    {
        // NO bloqueamos el shrine de forma permanente: se puede usar N veces
        CobrarVidaPermanenteYMostrarRuleta();
        CloseAndReset();
    }

    /// <summary>
    /// Baja la VIDA MÁXIMA de forma permanente (estilo HealthDownPowerUp),
    /// actualiza UI y guarda en GameDataManager.
    /// </summary>
    private void CobrarVidaPermanenteYMostrarRuleta()
    {
        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;

        var pc = player.GetComponent<PlayerController>();
        var ph = player.GetComponent<PlayerHealth>();
        if (pc == null || ph == null) return;

        // --- Cálculo de nueva vida máxima, respetando un mínimo ---
        float newMax = Mathf.Max(minMaxHealth, ph.maxHealth - Mathf.Abs(vidaACobrar));

        // Si por clamps no cambia nada, no tiene sentido cobrar
        if (Mathf.Approximately(newMax, ph.maxHealth))
        {
            Debug.Log("LifeShrine: No se pudo reducir más la vida máxima (ya está en el mínimo).");
            return;
        }

        // Aplicar reducción permanente (patrón igual a HealthDownPowerUp) :contentReference[oaicite:2]{index=2}
        ph.maxHealth = newMax;
        ph.currentHealth = Mathf.Min(ph.currentHealth, ph.maxHealth);

        if (ph.healthUI != null)
        {
            ph.healthUI.Initialize(ph.maxHealth);
            ph.healthUI.UpdateHearts(ph.currentHealth);
        }

        // Guardar en GameDataManager para que persista entre salas
        var gdm = GameDataManager.Instance;
        if (gdm != null)
        {
            gdm.SavePlayerData(pc);
        }

        // Mostrar ruleta
        if (wheelUIController != null)
        {
            wheelUIController.MostrarRuleta();
        }
    }

    private void CloseAndReset()
    {
        StopAllCoroutines();
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (dialogueText) dialogueText.text = "";

        isDialogueActive = false;
        isTyping = false;
        anyDialogueBusy = false;
        dialogueIndex = 0;
        showedInsufficientLine = false;

        if (runtimeIconRenderer)
        {
            runtimeIconRenderer.enabled =
                playerInRange &&
                !isDialogueActive &&
                !blockedForNoHealthUntilExit;
        }
    }

    // ---------- Helpers ----------
    private string GetCurrentLine() =>
        dialogueData != null &&
        dialogueData.dialogueLines != null &&
        dialogueData.dialogueLines.Length > 0
            ? dialogueData.dialogueLines[Mathf.Clamp(dialogueIndex, 0, SafeLinesCount() - 1)]
            : "";

    private string GetCurrentLineForDisplay() =>
        (showedInsufficientLine ? noHealthMessage : GetCurrentLine());

    private int SafeLinesCount() =>
        (dialogueData != null && dialogueData.dialogueLines != null)
            ? dialogueData.dialogueLines.Length
            : 0;

    private float GetTypingSpeed() =>
        (dialogueData != null && dialogueData.typingSpeed > 0f)
            ? dialogueData.typingSpeed
            : Mathf.Max(0.001f, fallbackTypingSpeed);

    private bool IsLastLine() => dialogueIndex >= SafeLinesCount() - 1;

    /// <summary>
    /// Comprueba si todavía se puede bajar la vida máxima
    /// (es decir, si después de restar vidaACobrar seguimos por encima de minMaxHealth).
    /// </summary>
    private bool HasEnoughHealth()
    {
        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return false;

        var ph = player.GetComponent<PlayerHealth>();
        if (ph == null) return false;

        float newMax = Mathf.Max(minMaxHealth, ph.maxHealth - Mathf.Abs(vidaACobrar));
        // Si el nuevo max es igual al actual es que ya estamos en el mínimo → no se puede apostar más
        return !Mathf.Approximately(newMax, ph.maxHealth);
    }

    // Si alguna vez querés que la ruleta libere el altar al terminar (evento externo),
    // podés seguir llamando a esto, pero ya no es estrictamente necesario.
    public void LiberarShrine()
    {
        // Dejado vacío a propósito: ya no usamos isShrineLocked.
        if (runtimeIconRenderer)
            runtimeIconRenderer.enabled = playerInRange && !isDialogueActive;
    }
}
