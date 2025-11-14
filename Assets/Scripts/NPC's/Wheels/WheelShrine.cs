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
    private bool showedInsufficientLine;
    private SpriteRenderer runtimeIconRenderer;
    private static bool anyDialogueBusy = false;
    private bool isShrineLocked = false;

    // NUEVO: bloqueo por falta de perks hasta salir del trigger
    private bool blockedForNoPerksUntilExit = false;

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
                !isShrineLocked &&
                !blockedForNoPerksUntilExit)
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
                    // Primera vez: mostrar aviso
                    if (!showedInsufficientLine)
                    {
                        showedInsufficientLine = true;
                        StartTypingNewLine(noPerksMessage, GetTypingSpeed());
                    }
                    else
                    {
                        // Segunda vez: cerrar y bloquear hasta salir
                        blockedForNoPerksUntilExit = true;
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
                !isShrineLocked &&
                !blockedForNoPerksUntilExit)
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
            blockedForNoPerksUntilExit = false;
            CloseAndReset();
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
        isShrineLocked = true;
        CobrarPerksYMostrarRuleta();
        CloseAndReset();
    }

    private void CobrarPerksYMostrarRuleta()
    {
        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;

        var pc = player.GetComponent<PlayerController>();
        if (pc == null) return;

        var actuales = new List<PowerUp>();
        if (pc.initialPowerUps != null && pc.initialPowerUps.Length > 0)
            actuales = pc.initialPowerUps.Where(p => p != null).ToList();

        if (actuales.Count < perksACobrar) return;

        var aRemover = ElegirPerksAleatorios(actuales, perksACobrar);

        var gdm = GameDataManager.Instance;
        if (gdm == null) return;

        foreach (var perk in aRemover)
        {
            if (perk == null) continue;
            gdm.RemovePerk(pc, perk);
        }

        var set = new HashSet<PowerUp>(aRemover);
        pc.initialPowerUps = (pc.initialPowerUps ?? new PowerUp[0])
            .Where(p => p != null && !set.Contains(p))
            .ToArray();

        try { gdm.SavePlayerData(pc); } catch { }

        if (wheelUIController != null)
            wheelUIController.MostrarRuleta();
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
            runtimeIconRenderer.enabled =
                playerInRange &&
                !isDialogueActive &&
                !isShrineLocked &&
                !blockedForNoPerksUntilExit;
    }

    // ---------- Helpers ----------
    private string GetCurrentLine() =>
        dialogueData != null &&
        dialogueData.dialogueLines != null &&
        dialogueData.dialogueLines.Length > 0 ?
        dialogueData.dialogueLines[Mathf.Clamp(dialogueIndex, 0, SafeLinesCount() - 1)] : "";

    private string GetCurrentLineForDisplay() =>
        (showedInsufficientLine ? noPerksMessage : GetCurrentLine());

    private int SafeLinesCount() =>
        (dialogueData != null && dialogueData.dialogueLines != null)
        ? dialogueData.dialogueLines.Length : 0;

    private float GetTypingSpeed() =>
        (dialogueData != null && dialogueData.typingSpeed > 0f)
        ? dialogueData.typingSpeed : Mathf.Max(0.001f, fallbackTypingSpeed);

    private bool IsLastLine() => dialogueIndex >= SafeLinesCount() - 1;

    private bool HasEnoughPerks()
    {
        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return false;

        var pc = player.GetComponent<PlayerController>();
        if (pc == null) return false;

        int count = pc.initialPowerUps?.Count(p => p != null) ?? 0;
        return count >= perksACobrar;
    }

    private static List<PowerUp> ElegirPerksAleatorios(List<PowerUp> source, int cantidad)
    {
        List<PowerUp> pool = new List<PowerUp>(source.Where(p => p != null));
        List<PowerUp> elegidos = new List<PowerUp>();
        for (int i = 0; i < cantidad && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            elegidos.Add(pool[index]);
            pool.RemoveAt(index);
        }
        return elegidos;
    }

    // Llamado cuando termina la ruleta
    public void LiberarShrine()
    {
        isShrineLocked = false;
        if (runtimeIconRenderer)
            runtimeIconRenderer.enabled = playerInRange && !isDialogueActive;
    }
}
