using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Collider2D))]
public class SlotMachineProximityPrompt : MonoBehaviour
{
    [Header("Sprite del prompt")]
    [Tooltip("Sprite que se mostrará encima de la máquina (asignalo desde el Inspector).")]
    public Sprite promptSprite;

    [Header("Ajustes visuales")]
    public Vector3 offset = new Vector3(0f, 1.5f, 0f);
    [Tooltip("Sorting Layer opcional; dejar vacío para usar la actual.")]
    public string sortingLayer = "";
    public int sortingOrder = 5000;

    [Header("Duración")]
    [Tooltip("Tiempo en segundos que el icono permanece visible.")]
    public float duration = 1f;

    [Header("Reaparición")]
    [Tooltip("Si está activado, vuelve a mostrar el icono cada vez que el player re-entre al trigger.")]
    public bool showOnEveryEnter = true;

    private bool isShowing = false;
    private bool canShow = true; // evita repetidos en el mismo 'enter' si hay varios colliders

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true; 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canShow || isShowing || promptSprite == null) return;

        // Detectar al jugador con tag o componente
        bool isPlayer = other.CompareTag("Player") || other.GetComponentInParent<PlayerController>() != null;
        if (!isPlayer) return;

        StartCoroutine(ShowOnce());
        if (!showOnEveryEnter) canShow = false;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!showOnEveryEnter) return;

        
        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerController>() != null)
            canShow = true;
    }

    private IEnumerator ShowOnce()
    {
        isShowing = true;

        var go = new GameObject("SlotMachinePromptIcon");
        go.transform.SetParent(transform, worldPositionStays: true);
        go.transform.position = transform.position + offset;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = promptSprite;
        if (!string.IsNullOrEmpty(sortingLayer))
            sr.sortingLayerName = sortingLayer;
        sr.sortingOrder = sortingOrder;

        yield return new WaitForSeconds(duration);

        if (go != null) Destroy(go);
        isShowing = false;

        yield return null;
    }
}
