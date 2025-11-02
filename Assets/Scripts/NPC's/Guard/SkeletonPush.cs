using UnityEngine;

public class SkeletonPush : MonoBehaviour
{
    [Header("Detección (rectángulo frontal)")]
    public Vector2 detectionBoxSize = new Vector2(1.5f, 1f);
    public Vector2 detectionOffset = new Vector2(0f, 1f); // local: (x->right, y->up)

    [Header("Empuje")]
    public float pushDistance = 0.5f;
    public float pushSpeed = 5f;
    public float pushCooldown = 1.5f;

    [Header("Referencias")]
    public Animator animator;
    public Transform player;
    [Tooltip("Opcional: referencia al PlayerController para leer IsDashing / isInvulnerable")]
    public Component playerController; // asume un componente con bool IsDashing / isInvulnerable
    [Tooltip("Opcional: hitbox del player si la deshabilitás durante el dash")]
    public Collider2D playerHitbox;

    [Header("Comportamiento")]
    [Tooltip("Si está activo, NO empuja cuando el player está en dash/invulnerable o ignorando colisiones.")]
    public bool skipPushWhenPlayerDashing = true;

    private bool canPush = true;
    private bool isPushing = false;

    // --- Helpers para reflejar campos del PlayerController sin acoplar fuerte ---
    private bool PlayerIsDashingOrInvuln()
    {
        if (playerController == null) return false;

        // Intentamos leer propiedades públicas comunes sin castear fuerte.
        // Si tu clase se llama PlayerController, podés castear y leer directo.
        // Aquí usamos reflexión liviana para evitar dependencia.
        var t = playerController.GetType();

        var isDashingProp = t.GetProperty("IsDashing");
        if (isDashingProp != null && isDashingProp.PropertyType == typeof(bool))
        {
            if ((bool)isDashingProp.GetValue(playerController, null)) return true;
        }

        var isInvulnField = t.GetField("isInvulnerable");
        if (isInvulnField != null && isInvulnField.FieldType == typeof(bool))
        {
            if ((bool)isInvulnField.GetValue(playerController)) return true;
        }

        var isInvulnProp = t.GetProperty("isInvulnerable");
        if (isInvulnProp != null && isInvulnProp.PropertyType == typeof(bool))
        {
            if ((bool)isInvulnProp.GetValue(playerController, null)) return true;
        }

        return false;
    }

    private bool GlobalPlayerEnemyCollisionIgnored()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (playerLayer < 0 || enemyLayer < 0) return false;
        return Physics2D.GetIgnoreLayerCollision(playerLayer, enemyLayer);
    }

    private bool ShouldSkipBecauseDash()
    {
        if (!skipPushWhenPlayerDashing) return false;

        // 1) Si el PlayerController indica dash o invulnerabilidad
        if (PlayerIsDashingOrInvuln()) return true;

        // 2) Si la hitbox del player está deshabilitada (suele pasar en dash)
        if (playerHitbox != null && !playerHitbox.enabled) return true;

        // 3) Si globalmente se ignoran colisiones Player-Enemy (tu DashState lo hace)
        if (GlobalPlayerEnemyCollisionIgnored()) return true;

        return false;
    }

    void Update()
    {
        if (player == null) return;

        // Si el player está en dash/passthrough, no detectamos ni empujamos
        if (ShouldSkipBecauseDash()) return;

        // Detección por overlap real (no por capas)
        Vector2 origin = (Vector2)transform.position +
                         (Vector2)(transform.right * detectionOffset.x + transform.up * detectionOffset.y);

        Collider2D[] hits = Physics2D.OverlapBoxAll(origin, detectionBoxSize, transform.eulerAngles.z);

        // ¿El rectángulo toca al collider del player?
        bool touchingPlayer = false;
        foreach (var h in hits)
        {
            if (h != null && (h.transform == player || h.transform.IsChildOf(player)))
            {
                touchingPlayer = true;
                break;
            }
        }

        if (touchingPlayer && canPush && !isPushing)
        {
            StartCoroutine(PushPlayer());
        }
    }

    private System.Collections.IEnumerator PushPlayer()
    {
        canPush = false;
        isPushing = true;

        if (animator != null) animator.SetTrigger("Push");
        yield return new WaitForSeconds(0.3f);

        // Empuje SOLO hacia atrás relativo al esqueleto
        Vector3 pushDir = -transform.up;
        Vector3 startPos = player.position;
        Vector3 targetPos = startPos + pushDir * pushDistance;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * pushSpeed;
            player.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        yield return new WaitForSeconds(pushCooldown);
        isPushing = false;
        canPush = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 origin = (Vector2)transform.position +
                         (Vector2)(transform.right * detectionOffset.x + transform.up * detectionOffset.y);
        Gizmos.matrix = Matrix4x4.TRS(origin, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, detectionBoxSize);
    }
}
