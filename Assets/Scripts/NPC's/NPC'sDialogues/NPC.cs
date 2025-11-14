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
    public Sprite interactIcon;
    [Tooltip("Opcional: SpriteRenderer de la escena si querés usar uno existente")]
    public SpriteRenderer interactRendererInScene;
    public float spriteOffsetY = 2f;
    public string playerTag = "Player";
    public string iconSortingLayer = "UI";
    public int iconSortingOrder = 50;

    // ===== Candado global de diálogo (uno a la vez) =====
    private static NPC currentActiveNPC = null;

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;
    private bool playerInRange;

    // Renderer creado en runtime
    private SpriteRenderer runtimeIconRenderer;

    // Cache animador UI
    private UIDialogueAnimator dialogueAnimator;

    // ===== NUEVO: Cierre por distancia =====
    [Header("Distancia máxima para mantener el diálogo")]
    public float maxDialogueDistance = 3.5f;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag(playerTag)?.transform;

        if (dialoguePanel)
        {
            dialoguePanel.SetActive(false);
            dialogueAnimator = dialoguePanel.GetComponent<UIDialogueAnimator>();
        }

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
        target.enabled = false;
    }

    void Update()
    {
        // Mantener icono en posición
        if (runtimeIconRenderer != null)
        {
            var pos = transform.position;
            pos.y += spriteOffsetY;
            runtimeIconRenderer.transform.position = pos;
        }

        UpdateIconVisibility();

        // Interacción
        if (playerInRange && Input.GetKeyDown(interactKey))
            Interact();

        // ===== NUEVO: cierre automático si se aleja demasiado =====
        if (isDialogueActive && player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);

            if (dist > maxDialogueDistance)
                EndDialogue();
        }
    }

    public bool CanInteract() => !isDialogueActive && currentActiveNPC == null;

    public void Interact()
    {
        if (currentActiveNPC != null && currentActiveNPC != this) return;

        if (isDialogueActive) NextLine();
        else StartDialogue();
    }

    void StartDialogue()
    {
        if (currentActiveNPC != null && currentActiveNPC != this) return;

        currentActiveNPC = this;
        isDialogueActive = true;
        dialogueIndex = 0;

        if (nameText != null) nameText.SetText(dialogueData.npcName);
        if (portraitImage != null) portraitImage.sprite = dialogueData.npcPortrait;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            dialogueAnimator ??= dialoguePanel.GetComponent<UIDialogueAnimator>();
            dialogueAnimator?.AnimateIn();
        }

        SetIconVisible(false);

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            if (dialogueText != null)
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
        if (dialogueText != null) dialogueText.SetText("");

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            if (dialogueText != null) dialogueText.text += letter;
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
        if (dialogueText != null) dialogueText.SetText("");

        if (dialoguePanel != null)
        {
            dialogueAnimator ??= dialoguePanel.GetComponent<UIDialogueAnimator>();
            if (dialogueAnimator != null)
            {
                dialogueAnimator.AnimateOut(() =>
                {
                    dialoguePanel.SetActive(false);

                    if (currentActiveNPC == this)
                        currentActiveNPC = null;

                    UpdateIconVisibility();
                });

                return;
            }
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (currentActiveNPC == this)
            currentActiveNPC = null;

        UpdateIconVisibility();
    }

    // ==== Proximidad física mediante trigger ====
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

            // ===== Nuevo: cerrar diálogo al salir del rango ====
            if (isDialogueActive)
                EndDialogue();
        }
    }

    // ===== Icono =====
    void UpdateIconVisibility()
    {
        bool visible = playerInRange && !isDialogueActive && currentActiveNPC == null;
        SetIconVisible(visible);
    }

    void SetIconVisible(bool visible)
    {
        if (interactRendererInScene != null) interactRendererInScene.enabled = visible;
        if (runtimeIconRenderer != null) runtimeIconRenderer.enabled = visible;
    }
}
