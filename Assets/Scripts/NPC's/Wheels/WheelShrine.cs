using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class WheelShrine : MonoBehaviour
{
    [Header("Diálogo (estilo NPC)")]
    public NPCDialogue dialogueData;     // nombre, retrato, dialogueLines[], typingSpeed
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

    [Header("Costo de entrada")]
    [Min(1)] public int perksACobrar = 2;
    [TextArea] public string noPerksMessage = "No tienes suficientes Perks para usar la ruleta.";

    [Header("Ruletas")]
    public EasyUI.PickerWheelUI.WheelUIController wheelUIController;

    // ---- Estado interno ----
    private bool playerInRange;
    private bool isDialogueActive;
    private bool isTyping;
    private int dialogueIndex;
    private bool showedInsufficientLine;     // mostró ya la línea de “no hay perks”
    private SpriteRenderer runtimeIconRenderer;
    private static bool anyDialogueBusy = false; // evita superponer diálogos con otros NPCs

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
            if (!isDialogueActive && !anyDialogueBusy)
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
                if (HasEnoughPerks())
                {
                    ExecuteRouletteAndClose();
                }
                else
                {
                    // Si no hay perks suficientes, mostramos 1 línea de aviso y luego cerramos en la siguiente pulsación
                    if (!showedInsufficientLine)
                    {
                        showedInsufficientLine = true;
                        StartTypingNewLine(noPerksMessage, GetTypingSpeed());
                    }
                    else
                    {
                        CloseAndReset();
                    }
                }
                return;
            }

            // Avanzar línea normal
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
            if (!isDialogueActive && runtimeIconRenderer) runtimeIconRenderer.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            CloseAndReset(); // cierra y limpia todo al salir del trigger
        }
    }

    // ---------- Diálogo ----------
    private void StartDialogue()
    {
        if (dialogueData == null || dialoguePanel == null || dialogueText == null)
        {
            Debug.LogError("WheelShrine: Faltan referencias de diálogo en el Inspector.");
            return;
        }

        StopAllCoroutines();                 // evita solapados
        anyDialogueBusy = true;
        isDialogueActive = true;
        isTyping = false;
        dialogueIndex = 0;
        showedInsufficientLine = false;

        if (nameText) nameText.SetText(dialogueData.npcName);
        if (portraitImage) portraitImage.sprite = dialogueData.npcPortrait;

        dialogueText.text = "";              // limpiar siempre
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
        StopAllCoroutines();          // no mezclar corutinas
        dialogueText.text = "";       // limpiar cuadro antes de cada tipeo
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

    // ---------- Ejecución de ruleta + costo ----------
    private void ExecuteRouletteAndClose()
    {
        CobrarPerksYMostrarRuleta();
        CloseAndReset();
    }

    private void CobrarPerksYMostrarRuleta()
    {
        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) { Debug.LogWarning("WheelShrine: Player no encontrado."); return; }

        var pc = player.GetComponent<PlayerController>();
        if (pc == null) { Debug.LogWarning("WheelShrine: PlayerController no encontrado."); return; }

        // Perks actuales no nulas
        var actuales = new List<PowerUp>();
        if (pc.initialPowerUps != null && pc.initialPowerUps.Length > 0)
            actuales = pc.initialPowerUps.Where(p => p != null).ToList();

        // Seguridad adicional
        if (actuales.Count < perksACobrar)
        {
            Debug.LogWarning($"WheelShrine: Se requieren {perksACobrar} perks, jugador tiene {actuales.Count}. Cancelado.");
            return;
        }

        // Elegimos aleatorias
        var aRemover = ElegirPerksAleatorios(actuales, perksACobrar);

        // USAR GameDataManager PARA BORRAR EFECTOS Y SACARLAS DEL SAVE
        var gdm = GameDataManager.Instance;
        if (gdm == null)
        {
            Debug.LogError("WheelShrine: GameDataManager.Instance es null.");
            return;
        }

        // Log de auditoría (opcional para test)
        var nombres = new List<string>();

        foreach (var perk in aRemover)
        {
            if (perk == null) continue;
            nombres.Add(perk.name);
            // Esto llama perk.Remove(player) adentro y limpia efectos + las saca de playerData.initialPowerUps
            gdm.RemovePerk(pc, perk);
        }

        // Reflejar la eliminación también en el array del Player (runtime actual)
        var setRemovidas = new HashSet<PowerUp>(aRemover.Where(p => p != null));
        pc.initialPowerUps = (pc.initialPowerUps ?? new PowerUp[0])
            .Where(p => p != null && !setRemovidas.Contains(p))
            .ToArray();

        // Guardado rápido (opcional)
        try { gdm.SavePlayerData(pc); } catch { /* opcional, silencioso */ }

        if (nombres.Count > 0)
            Debug.Log($"[WheelShrine] Perks eliminadas: {string.Join(", ", nombres)}");

        // Mostrar ruleta
        if (wheelUIController == null)
            wheelUIController = FindFirstObjectByType<EasyUI.PickerWheelUI.WheelUIController>();

        if (wheelUIController != null)
            wheelUIController.MostrarRuleta();
        else
            Debug.LogWarning("WheelShrine: WheelUIController no encontrado en escena.");
    }

    private void CloseAndReset()
    {
        StopAllCoroutines();              // <— corta cualquier tipeo en curso
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (dialogueText) dialogueText.text = "";  // <— limpia texto para no “mezclar”
        isDialogueActive = false;
        isTyping = false;
        anyDialogueBusy = false;
        dialogueIndex = 0;
        showedInsufficientLine = false;

        // Volver a mostrar el icono si el player sigue dentro
        if (runtimeIconRenderer) runtimeIconRenderer.enabled = playerInRange && !isDialogueActive;
    }

    // ---------- Helpers de diálogo ----------
    private string GetCurrentLine()
    {
        int idx = Mathf.Clamp(dialogueIndex, 0, SafeLinesCount() - 1);
        return dialogueData != null && dialogueData.dialogueLines != null && dialogueData.dialogueLines.Length > 0
            ? dialogueData.dialogueLines[idx]
            : "";
    }

    // Para mostrar instantáneamente la línea actual si se interrumpe el tipeo
    private string GetCurrentLineForDisplay()
    {
        return IsShowingInsufficientLine() ? noPerksMessage : GetCurrentLine();
    }

    private int SafeLinesCount()
    {
        return (dialogueData != null && dialogueData.dialogueLines != null)
            ? dialogueData.dialogueLines.Length
            : 0;
    }

    private float GetTypingSpeed()
    {
        return (dialogueData != null && dialogueData.typingSpeed > 0f)
            ? dialogueData.typingSpeed
            : Mathf.Max(0.001f, fallbackTypingSpeed);
    }

    private bool IsLastLine()
    {
        return SafeLinesCount() == 0 || dialogueIndex >= SafeLinesCount() - 1;
    }

    private bool IsShowingInsufficientLine()
    {
        // Se considera “línea especial” cuando ya mostramos el aviso y no estamos tipeando
        return showedInsufficientLine && (dialogueText != null && dialogueText.text == noPerksMessage);
    }

    private bool HasEnoughPerks()
    {
        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return false;

        var pc = player.GetComponent<PlayerController>();
        if (pc == null) return false;

        int count = (pc.initialPowerUps == null) ? 0 : pc.initialPowerUps.Count(p => p != null);
        return count >= perksACobrar;
    }

    // ---------- Utilidades de perks ----------
    private static List<PowerUp> ElegirPerksAleatorios(List<PowerUp> source, int cantidad)
    {
        List<PowerUp> pool = new List<PowerUp>(source.Where(p => p != null));
        List<PowerUp> elegidos = new List<PowerUp>(cantidad);
        for (int i = 0; i < cantidad && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            elegidos.Add(pool[index]);
            pool.RemoveAt(index);
        }
        return elegidos;
    }
}