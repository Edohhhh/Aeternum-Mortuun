using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPC : MonoBehaviour
{
    [Header("Datos de diálogo")]
    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;

    [Header("Interacción")]
    public KeyCode interactKey = KeyCode.F;
    [Tooltip("Sprite (asset) que indica que se puede interactuar")]
    public Sprite interactIcon;                       // Asset sprite
    [Tooltip("Opcional: SpriteRenderer de la escena si querés usar uno existente")]
    public SpriteRenderer interactRendererInScene;    // Renderer existente (opcional)
    public float spriteOffsetY = 2f;
    public string playerTag = "Player";
    public string iconSortingLayer = "UI";
    public int iconSortingOrder = 50;

    // ===== Candado global de diálogo =====
    private static NPC currentActiveNPC = null; // null => nadie hablando

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;
    private bool playerInRange;

    // Renderer creado en runtime si no hay uno en escena
    private SpriteRenderer runtimeIconRenderer;

    void Start()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);

        // Preparar icono
        SpriteRenderer target = interactRendererInScene;
        if (target == null)
        {
            var go = new GameObject("InteractIcon");
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0f, spriteOffsetY, 0f);
            target = go.AddComponent<SpriteRenderer>();
            runtimeIconRenderer = target;
        }

        if (interactIcon != null) target.sprite = interactIcon;
        target.sortingLayerName = iconSortingLayer;
        target.sortingOrder = iconSortingOrder;
        target.enabled = false; // oculto al inicio
    }

    void Update()
    {
        // Mantener posición del icono runtime
        if (runtimeIconRenderer != null)
        {
            var pos = transform.position;
            pos.y += spriteOffsetY;
            runtimeIconRenderer.transform.position = pos;
        }

        // Mostrar/ocultar icono según estado global/local
        UpdateIconVisibility();

        // Tecla de interacción
        if (playerInRange && Input.GetKeyDown(interactKey))
            Interact();
    }

    public bool CanInteract() => !isDialogueActive && currentActiveNPC == null;

    public void Interact()
    {
        // Si hay otro diálogo activo, no hacer nada
        if (currentActiveNPC != null && currentActiveNPC != this) return;

        if (isDialogueActive) NextLine();
        else StartDialogue();
    }

    void StartDialogue()
    {
        // Evitar doble inicio si alguien ya está hablando
        if (currentActiveNPC != null && currentActiveNPC != this) return;

        currentActiveNPC = this;     // marcar globalmente
        isDialogueActive = true;
        dialogueIndex = 0;

        nameText.SetText(dialogueData.npcName);
        portraitImage.sprite = dialogueData.npcPortrait;

        dialoguePanel.SetActive(true);
        SetIconVisible(false);       // ocultar icono mientras se habla

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping = false;

        if (dialogueData.autoProgressLines.Length > dialogueIndex &&
            dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueText.SetText("");
        dialoguePanel.SetActive(false);

        // Liberar el candado global SOLO si este NPC era el activo
        if (currentActiveNPC == this)
            currentActiveNPC = null;

        // Si el jugador sigue cerca y no hay otro diálogo activo, mostrar icono
        UpdateIconVisibility();
    }

    // ==== Proximidad (2D). Para 3D usa OnTriggerEnter/Exit (no 2D) ====
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            UpdateIconVisibility();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            UpdateIconVisibility();
        }
    }

    // ===== Utilidades de icono =====
    void UpdateIconVisibility()
    {
        // El icono solo se muestra si: jugador cerca, este NPC NO está hablando y NO hay otro diálogo activo
        bool visible = playerInRange && !isDialogueActive && currentActiveNPC == null;
        SetIconVisible(visible);
    }

    void SetIconVisible(bool visible)
    {
        if (interactRendererInScene != null) interactRendererInScene.enabled = visible;
        if (runtimeIconRenderer != null) runtimeIconRenderer.enabled = visible;
    }
}
