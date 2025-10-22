using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TrainingDummyController : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 1.5f;
    [SerializeField] private float knockbackDuration = 0.1f;

    [Header("Return Settings")]
    [SerializeField] private float returnSpeed = 10f;
    [SerializeField] private Transform checkpoint; // punto de retorno

    private Rigidbody2D rb;
    private Vector2 fallbackOriginalPos;
    private bool isKnockedBack = false;
    private Coroutine returnCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Configuración física segura
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // 🔹 Evita que el dummy sea empujado
        rb.sharedMaterial = new PhysicsMaterial2D { friction = 0f, bounciness = 0f };
    }

    private void Start()
    {
        fallbackOriginalPos = rb.position;
        StartCoroutine(ForceNoGravity());
    }

    private IEnumerator ForceNoGravity()
    {
        for (int i = 0; i < 5; i++)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            yield return null;
        }
    }

    public void OnHit(Vector2 attackerPosition)
    {
        if (isKnockedBack) return;

        isKnockedBack = true;
        Vector2 direction = (rb.position - attackerPosition).normalized;

        // Aplicar knockback
        rb.linearVelocity = direction * knockbackForce;

        if (returnCoroutine != null)
            StopCoroutine(returnCoroutine);

        returnCoroutine = StartCoroutine(ReturnToCheckpoint());
    }

    private IEnumerator ReturnToCheckpoint()
    {
        yield return new WaitForSeconds(knockbackDuration);

        Vector2 target = checkpoint != null ? (Vector2)checkpoint.position : fallbackOriginalPos;

        // Movimiento suave hacia el checkpoint
        while (Vector2.Distance(rb.position, target) > 0.01f)
        {
            Vector2 newPos = Vector2.MoveTowards(rb.position, target, returnSpeed * Time.deltaTime);
            rb.MovePosition(newPos);
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        rb.position = target;
        isKnockedBack = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector2 target = checkpoint != null ? (Vector2)checkpoint.position : fallbackOriginalPos;
        Gizmos.DrawWireSphere(target, 0.05f);
        Gizmos.DrawLine(transform.position, target);
    }
}
