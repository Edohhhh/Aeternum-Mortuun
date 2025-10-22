using UnityEngine;

public class SkeletonPush : MonoBehaviour
{
    [Header("Configuración")]
    public float detectionRange = 1.5f;   // Distancia para detectar al jugador
    public float pushDistance = 0.5f;     // Cuánto se mueve el jugador al ser empujado
    public float pushSpeed = 5f;          // Velocidad del empuje (más alto = más rápido)
    public float pushCooldown = 1.5f;     // Tiempo entre empujones

    [Header("Referencias")]
    public Animator animator;
    public Transform player;

    private bool canPush = true;
    private bool isPushing = false;

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Si el jugador está cerca y el esqueleto puede empujar
        if (distance <= detectionRange && canPush && !isPushing)
        {
            StartCoroutine(PushPlayer());
        }
    }

    private System.Collections.IEnumerator PushPlayer()
    {
        canPush = false;
        isPushing = true;

        // Activa animación
        if (animator != null)
            animator.SetTrigger("Push");

        yield return new WaitForSeconds(0.3f); // pequeño delay para sincronizar con animación

        // Calcula dirección y posición destino
        Vector3 pushDir = (player.position - transform.position).normalized;
        Vector3 targetPos = player.position + pushDir * pushDistance;

        // Desplaza al jugador suavemente
        float elapsed = 0f;
        Vector3 startPos = player.position;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * pushSpeed;
            player.position = Vector3.Lerp(startPos, targetPos, elapsed);
            yield return null;
        }

        // Espera antes de permitir otro empuje
        yield return new WaitForSeconds(pushCooldown);
        canPush = true;
        isPushing = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
